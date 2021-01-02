﻿using System.IO;
using Newtonsoft.Json;

namespace Translation
{
    public static class JsonHelper
    {
        public static T Deserialize<T>(string fileName)
        {
            using StreamReader file = File.OpenText(fileName);
            JsonSerializer serializer = new JsonSerializer();
            return (T) serializer.Deserialize(file, typeof(T));
        }

        public static void Populate(string fileName, object obj)
        {
            using StreamReader file = File.OpenText(fileName);
            JsonSerializer serializer = new JsonSerializer();
            serializer.Populate(file, obj);
        }

        public static void Serialize(object obj, string fileName)
        {
            using StreamWriter file = File.CreateText(fileName);
            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented
            };
            JsonSerializer serializer = JsonSerializer.Create(settings);
            serializer.Serialize(file, obj);
        }
    }
}