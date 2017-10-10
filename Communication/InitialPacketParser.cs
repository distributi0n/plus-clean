﻿namespace Plus.Communication
{
    using System;
    using ConnectionManager;

    public class InitialPacketParser : IDataParser
    {
        public delegate void NoParamDelegate();

        public byte[] currentData;

        public void handlePacketData(byte[] packet)
        {
            if (packet[0] == 60 && PolicyRequest != null)
            {
                PolicyRequest.Invoke();
            }
            else if (packet[0] != 67 && SwitchParserRequest != null)
            {
                currentData = packet;
                SwitchParserRequest.Invoke();
            }
        }

        public void Dispose()
        {
            PolicyRequest = null;
            SwitchParserRequest = null;
            GC.SuppressFinalize(this);
        }

        public object Clone() => new InitialPacketParser();

        public event NoParamDelegate PolicyRequest;
        public event NoParamDelegate SwitchParserRequest;
    }
}