﻿// Copyright (c) Oleg Zudov. All Rights Reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// This file is based on or incorporates material from the chrome-dev-tools-sample, licensed under the MIT License. More info in THIRD-PARTY-NOTICES file.

using Newtonsoft.Json;
namespace AsyncChromeDriverConsoleApp
{

    public class ChromeSessionInfo
    {
        [JsonProperty("description")]
        public string Description
        {
            get;
            set;
        }

        [JsonProperty("devtoolsFrontendUrl")]
        public string DevToolsFrontendUrl
        {
            get;
            set;
        }

        [JsonProperty("id")]
        public string Id
        {
            get;
            set;
        }

        [JsonProperty("title")]
        public string Title
        {
            get;
            set;
        }

        [JsonProperty("type")]
        public string Type
        {
            get;
            set;
        }

        [JsonProperty("webSocketDebuggerUrl")]
        public string WebSocketDebuggerUrl
        {
            get;
            set;
        }
    }
}
