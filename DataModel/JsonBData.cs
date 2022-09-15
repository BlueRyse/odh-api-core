﻿using System;
using System.Collections.Generic;
using System.Text;

namespace DataModel
{
    public class JsonBData
    {
        public string? id { get; set; }
        public JsonRaw? data { get; set; }
    }

    public class JsonBDataRaw
    {
        public string id { get; set; }
        public JsonRaw data { get; set; }
        public Int32 rawdataid { get; set; }
    }

    public interface IRawDataStore 
    {
        string type { get; set; }
        string datasource { get; set; }
        string sourceinterface { get; set; }
        string sourceid { get; set; }
        string sourceurl { get; set; }
        DateTime importdate { get; set; }
        string license { get; set; }
        string rawformat { get; set; }        
    }

    public class RawDataStore : IRawDataStore
    {        
        public string type { get; set; }
        public string datasource { get; set; }
        public string sourceinterface { get; set; }
        public string sourceid { get; set; }
        public string sourceurl { get; set; }
        public DateTime importdate { get; set; }

        public string license { get; set; }
        public string rawformat { get; set; }

        public string raw { get; set; }        
    }

    public class RawDataStoreJson : RawDataStore
    {      
        public RawDataStoreJson(RawDataStore rawdatastore)
        {
            this.importdate = rawdatastore.importdate;
            this.datasource = rawdatastore.datasource;
            this.rawformat = rawdatastore.rawformat;
            this.sourceid = rawdatastore.sourceid;
            this.sourceinterface = rawdatastore.sourceinterface;
            this.sourceurl = rawdatastore.sourceurl;
            this.type = rawdatastore.type;
            this.license = rawdatastore.license;

            this.raw = new JsonRaw(rawdatastore.raw);
        }

        public new JsonRaw raw { get; set; }
    }

    public static class RawDataStoreExtensions
    {
        public static IRawDataStore UseJsonRaw(this RawDataStore rawdatastore)
        {
            if(rawdatastore.rawformat == "json")
            {
                return new RawDataStoreJson(rawdatastore);
            }
            else
                return rawdatastore;
        }
    }
}

