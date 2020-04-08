﻿//******************************************************************************
// <copyright file="license.md" company="RawCMS project  (https://github.com/arduosoft/RawCMS)">
// Copyright (c) 2019 RawCMS project  (https://github.com/arduosoft/RawCMS)
// RawCMS project is released under GPL3 terms, see LICENSE file on repository root at  https://github.com/arduosoft/RawCMS .
// </copyright>
// <author>Daniele Fontani, Emanuele Bucarelli, Francesco Mina'</author>
// <autogenerated>true</autogenerated>
//******************************************************************************
using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace RawCMS.Library.Core.Helpers
{
    public class ApplicationLogger
    {
        private static ILoggerFactory _loggerFactory;

        #region fun

        public static string WelcomeMessage
        {
            get
            {
                return

@"
MMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMM
MMMMMMMMMMMMWNMMMMMMMMMMMMMMMMMMMM ______               _____                M
MWNXWMMMMMMKc,dXMMMMMMMMMMMMMMMMMM | ___ \             /  __ \               M
MWXKXNMMMW0;...cKWMMMMMMMMMMMMMMMM | |_/ /__ ___      _| /  \/_ __ ___  ___  M
MWXKKXMMMNkoxd,.':o0WMMMMMMMWMWMWM |    // _` \ \ /\ / / |   | '_ ` _ \/ __| M
MMMMMMMMXkONN0;    .lXMMMMMMKMKMKM | |\ \ (_| |\ V  V /| \__/\ | | | | \__ \ M
MMMMMMMK:.:kd,.   . .dXNWMMMKMKMKM \_| \_\__,_| \_/\_/  \____/_| |_| |_|___/ M
MMMMMMMO' ... .   . .ll,dXMMKMKMKM                                           M
MMMMMMM0' ......    ,o' .xWMKMKMKM       _____  _ _            _             M
MMMMMMMX: .;:;,.   .l;. .xWMKMKMKM      /  __ \| (_)          | |            M
MMMMMMMWd. .dO, . .:;,' ,0WMKMKMKM      | /  \/| |_  ___ _ __ | |_           M
MMMMMMMMK; .dX: .,kl.;o'.lXMKMKMKM      | |   || | |/ _ \ '_ \| __|          M
MMMMMMMMWO,.c0c .cKd,cOc .xMKMKMKM      | \__/\| | |  __/ | | | |_           M
MMMMMMMMMWk,;kd,,l0XXNXdcl0WKWKWKW       \____/|_|_|\___|_| |_|\__|          M
MMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMM
";
            }
        }

        #endregion fun

        public static ILoggerFactory LoggerFactory { get => _loggerFactory; set => _loggerFactory = value; }

        public static void SetLogFactory(ILoggerFactory loggerFactory)
        {
            LoggerFactory = loggerFactory;
            var logger = CreateLogger<ApplicationLogger>();
            logger.LogInformation(ApplicationLogger.WelcomeMessage);
            Debug.Write(ApplicationLogger.WelcomeMessage);
        }

        public static ILogger CreateLogger<T>()
        {
            return LoggerFactory.CreateLogger<T>();
        }

        public static ILogger CreateLogger(string name)
        {
            return LoggerFactory.CreateLogger(name);
        }

        public static NLog.Logger CreateRawLogger(string env)
        {
            string path = GetConfigPath(env);
            return NLog.Web.NLogBuilder.ConfigureNLog(path).GetCurrentClassLogger();
        }

        public static string GetConfigPath(string env)
        {
            if (env != null)
            {
                env = "." + env;
            }
            string path = $"./conf/NLog{env}.config";
            return path;
        }
    }
}