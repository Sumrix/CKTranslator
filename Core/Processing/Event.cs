﻿namespace Core.Processing
{
    public enum EventType
    {
        Info,
        Error,
        Warning
    }

    public class Event
    {
        public string Description { get; init; } = "";
        public string FileName { get; init; } = "";
        public string ModName { get; set; } = "";
        public EventType Type { get; init; }
    }
}