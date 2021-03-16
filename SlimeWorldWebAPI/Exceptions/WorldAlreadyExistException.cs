using System;

namespace SlimeWorldWebAPI.Exceptions
{
    public class WorldAlreadyExistException : Exception
    {
        public WorldAlreadyExistException(string worldName) : base($"World already exists {worldName}")
        {
            
        }
    }
}
