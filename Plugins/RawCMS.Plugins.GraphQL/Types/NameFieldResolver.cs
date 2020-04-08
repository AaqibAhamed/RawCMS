﻿//******************************************************************************
// <copyright file="license.md" company="RawCMS project  (https://github.com/arduosoft/RawCMS)">
// Copyright (c) 2019 RawCMS project  (https://github.com/arduosoft/RawCMS)
// RawCMS project is released under GPL3 terms, see LICENSE file on repository root at  https://github.com/arduosoft/RawCMS .
// </copyright>
// <author>Daniele Fontani, Emanuele Bucarelli, Francesco Mina'</author>
// <autogenerated>true</autogenerated>
//******************************************************************************
using System;
using GraphQL.Resolvers;
using GraphQL.Types;
using Newtonsoft.Json.Linq;

namespace RawCMS.Plugins.GraphQL.Types
{
    public class NameFieldResolver : IFieldResolver
    {
        public object Resolve(ResolveFieldContext context)
        {
            object source = context.Source;
            if (source == null)
            {
                return null;
            }
            string name = char.ToUpperInvariant(context.FieldAst.Name[0]) + context.FieldAst.Name.Substring(1);
            object value;
            if (context.SubFields != null && context.SubFields.Count > 0)
            {
                JObject src = source as JObject;
                var token = src.SelectToken($"$._metadata.rel.{name}", false);
                //value = token.Value<object>();
                if (token is JArray)
                {
                    value = token.Value<object>();
                }
                else
                {
                    value = new JArray(token.Value<object>());
                }
            }
            else
            {
                value = GetPropValue(source, name);
            }

            if (value == null)
            {
                throw new InvalidOperationException($"Expected to find property {context.FieldAst.Name} on {context.Source.GetType().Name} but it does not exist.");
            }
            return value;
        }

        private static object GetPropValue(object src, string propName)
        {
            try
            {
                JObject source = src as JObject;
                JToken value;
                source.TryGetValue(propName, StringComparison.InvariantCultureIgnoreCase, out value);
                if (value != null)
                {
                    return value.Value<object>();
                }
                else
                {
                    return null;
                }
            }
            catch (Exception e)
            {
                throw;
            }
        }
    }
}