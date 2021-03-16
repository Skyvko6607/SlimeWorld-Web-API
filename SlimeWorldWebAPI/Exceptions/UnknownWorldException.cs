using System;

namespace SlimeWorldWebAPI.Exceptions
{
    public class UnknownWorldException : Exception
    {
        public UnknownWorldException(string worldName) : base($"World not found {worldName}")
        {
        }
    }
}