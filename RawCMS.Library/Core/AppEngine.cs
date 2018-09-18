﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Logging;
using RawCMS.Library.Service;
using RawCMS.Library.Core.Interfaces;
using RawCMS.Library.Core.Extension;
using System.IO;

namespace RawCMS.Library.Core
{
    public class AppEngine
    {
        //#region singleton
        //private static LambdaManager _current = null;
        private static ILogger _logger;
        private ILoggerFactory loggerFactory;

        public ILogger GetLogger(object caller)
        {
           return this.loggerFactory.CreateLogger(caller.GetType());
        }

        //public static void SetLogger(ILoggerFactory factory)
        //{
        //    logger = factory.CreateLogger(typeof(LambdaManager));
        //}
        //public static LambdaManager Current
        //{
        //    get
        //    {
        //        return _current ?? (_current= new LambdaManager() );
        //    }
        //}
        //#endregion


       

        public CRUDService Service { get { return service; } }

        public List<Lambda> Lambdas { get; set; } = new List<Lambda>();
        //public Lambda this[string name]
        //{
        //    get
        //    {
        //        return Lambdas.FirstOrDefault(x => x.Name == name);
        //    }
        //}

        public List<Plugin> Plugins { get; set; } = new List<Plugin>();
        //public Plugin this[string name]
        //{
        //    get
        //    {
        //        return Plugin.FirstOrDefault(x => x.Name == name);
        //    }
        //}



        public AppEngine(ILoggerFactory loggerFactory,CRUDService service)
        {
            _logger = loggerFactory.CreateLogger(typeof(AppEngine));
            this.loggerFactory = loggerFactory;
            this.service = service;
            this.service.SetAppEngine(this);//TODO: fix this circular dependemcy
           // LoadAllAssembly();
            LoadLambdas();
            LoadPlugins();
        }

        private void LoadPlugins()
        {
           Plugins= GetAnnotatedInstances<Plugin>();
        }


        private readonly CRUDService service;
        private void LoadLambdas()
        {
           
            DiscoverLambdasInBundle();
        }

        public List<string> GetAllAssembly()
        {
            List<string> dlls = new List<string>();
            dlls.AddRange(Directory.GetFiles(".\\bin", "*.dll", SearchOption.AllDirectories));
            return dlls;
            
        }

        public void LoadAllAssembly()
        {
            //foreach (var type in Assembly.GetExecutingAssembly().GetTypes())
            //{
            //    foreach (var method in type.GetMethods(BindingFlags.DeclaredOnly |
            //                        BindingFlags.NonPublic |
            //                        BindingFlags.Public | BindingFlags.Instance |
            //                        BindingFlags.Static))
            //    {
            //        System.Runtime.CompilerServices.RuntimeHelpers.PrepareMethod(method.MethodHandle);
            //    }
            //}

            GetAllAssembly().ForEach(x => Assembly.LoadFrom(x));
        }

            public T GetInstance<T>(params object[] args) where T:class
        {
            return Activator.CreateInstance(typeof(T), args) as T;
        }

        public T GetInstance<T>(Type type,params object[] args) where T : class
        {
            return Activator.CreateInstance(type, args) as T ;
        }

        public List<Type> GetAnnotatedBy<T>() where T : Attribute
        {
            List<Type> result = new List<Type>();
            List<Assembly> bundledAssemblies = GetAssemblyInScope();
            foreach (var assembly in bundledAssemblies)
            {
                _logger.LogInformation("loading from" + assembly.FullName);
                var types = assembly.GetTypes();


                foreach (var type in types)
                {
                    if (type.GetCustomAttributes(typeof(T), true).Length > 0)
                    {
                        result.Add(type);
                    }
                }
            }
            return result;
        }


        /// <summary>
        /// Get  instances of all classes assignable from T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public List<T> GetAssignablesInstances<T>() where T : class
        {
            List<Type> types = GetImplementors<T>();
            return GetInstancesFromTypes<T>(types);
        }

        /// <summary>
        /// Get instanced of all classes annotated by T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public List<T> GetAnnotatedInstances<T>() where T : class
        {
            List<Type> types = GetImplementors<T>();
            return GetInstancesFromTypes<T>(types);
        }

        /// <summary>
        /// Get all types that implements T or inherit it
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public List<Type> GetImplementors<T>() where T : class
        {
            return GetImplementors(typeof(T), GetAssemblyInScope());
        }

        private List<Type> GetImplementors(Type t, List<Assembly> bundledAssemblies) 
        {
            List<Type> result = new List<Type>();
            
            foreach (var assembly in bundledAssemblies)
            {
                _logger.LogInformation("loading from" + assembly.FullName);
                var types = assembly.GetTypes();


                foreach (var type in types)
                {
                    try
                    {
                        if (t.IsAssignableFrom(type) && !type.IsAbstract && !type.IsInterface)
                        {
                            result.Add(type);
                        }
                    }
                    catch (Exception err)
                    {
                        _logger.LogError(err, "- (unable to create an instance for EXCEPTION skipped) - " + type.Name + " | " + type.GetType().FullName);
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Get all assemblies that may contains T instances
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public List<Assembly> GetAssemblyInScope() 
        {
            //TODO: use configuration to define assembly map or regexp to define where to lookup
            return AppDomain.CurrentDomain.GetAssemblies().Where(x => x.GetName().Name.ToLower().StartsWith("rawcms.")).ToList();
        }

        public List<Assembly> GetAssemblyWithInstance() 
        {
            List<Assembly> result = new List<Assembly>();
            var assList = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var ass in assList)
            {
                var implementors=this.GetImplementors(typeof(Plugin), new List<Assembly>() { ass});
                if (implementors.Count > 0)
                {
                    result.Add(ass);
                }
            }
            return result;
        }


            /// <summary>
            /// give instances of a list of types
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="types"></param>
            /// <returns></returns>
            public List<T> GetInstancesFromTypes<T>(List<Type> types) where T : class
        {
            List<T> result = new List<T>();
            
            types.ForEach(x =>
            {
                result.Add(this.GetInstance<T>(x));
            });           

            return result;
        }
        /// <summary>
        /// Find and load all lambas already loaded with main bundle (no dinamycs)
        /// </summary>
        private void DiscoverLambdasInBundle()
        {

            List<Lambda> lambdas = GetAnnotatedInstances<Lambda>();

            foreach (Lambda instance in lambdas)
            {
                if (instance != null)
                {
                    if (instance is IRequireApp)
                    {
                        ((IRequireApp)instance).SetAppEngine(this);
                    }

                    if (instance is IRequireCrudService)
                    {
                        ((IRequireCrudService)instance).SetCRUDService(this.Service);
                    }

                    if (instance is IInitable)
                    {

                        ((IInitable)instance).Init();
                    }

                    Lambdas.Add(instance);

                    _logger.LogInformation("-" + instance.Name + " | " + instance.GetType().FullName);
                }

            }
        }
                    
               
            
        
    }
}