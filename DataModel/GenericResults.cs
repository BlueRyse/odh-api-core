﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataModel
{
    public struct UpdateResult
    {
        public string operation { get; init; }
        public string updatetype { get; init; }
        public string otherinfo { get; init; }
        public string message { get; init; }
        public bool success { get; init; }
        public int? recordsmodified { get; init; }

        public int? updated { get; init; }
        public int? created { get; init; }
        public int? deleted { get; init; }

        public int? error { get; init; }

        public string id { get; init; }

        public string exception { get; init; }

        public string stacktrace { get; init; }

        public string source { get; init; }
    }

    public struct UpdateDetail
    {
        public int? updated { get; init; }
        public int? created { get; init; }
        public int? deleted { get; init; }

        public int? error { get; init; }
    }

    public struct PGCRUDResult
    {
        public string id { get; init; }
        public string operation { get; init; }
        public int? updated { get; init; }
        public int? created { get; init; }
        public int? deleted { get; init; }

        public int? error { get; init; }
    }

    public class GenericResultsHelper
    {
        public static UpdateDetail MergeUpdateDetail(IEnumerable<UpdateDetail> updatedetails)
        {
            int? updated = 0;
            int? created = 0;
            int? deleted = 0;
            int? error = 0;

            foreach (var updatedetail in updatedetails)
            {
                    created = updatedetail.created + created;
                    updated = updatedetail.updated + updated;
                    deleted = updatedetail.deleted + deleted;
                    error = updatedetail.error + error;                
            }

            return new UpdateDetail() { created = created, updated = updated, deleted = deleted, error= error };
        }

        public static UpdateResult GetSuccessUpdateResult(string id, string source, string operation, string updatetype, string message, string otherinfo, UpdateDetail detail, bool createlog)
        {
            var result = new UpdateResult()
            {
                id = id,
                source = source,
                operation = operation,
                updatetype = updatetype,
                otherinfo = otherinfo,
                message = message,
                recordsmodified = (detail.created + detail.updated + detail.deleted),
                created = detail.created,
                updated = detail.updated,
                deleted = detail.deleted,
                error = detail.error,
                success = true,
                exception = null,
                stacktrace = null
            };

            if(createlog)
                Console.WriteLine(JsonConvert.SerializeObject(result));

            return result;
        }

        public static UpdateResult GetErrorUpdateResult(string id, string source, string operation, string updatetype, string message, string otherinfo, UpdateDetail detail, Exception ex, bool createlog)
        {    
            var result = new UpdateResult()
            {
                id = id,
                source = source,
                operation = operation,
                updatetype = updatetype,
                otherinfo = "",
                message = message,
                recordsmodified = (detail.created + detail.updated + detail.deleted),
                created = detail.created,
                updated = detail.updated,
                deleted = detail.deleted,
                error = detail.error,
                success = false,
                exception = ex.Message,
                stacktrace = ex.StackTrace
            };

            if (createlog)
                Console.WriteLine(JsonConvert.SerializeObject(result));

            return result;
        }
    }
}
