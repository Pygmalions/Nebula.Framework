using System;
using System.Reflection;
using System.Runtime.InteropServices;
using Nebula.Proxying;
using Nebula.Proxying.Utilities;
using NUnit.Framework;

namespace Nebula.Prism.Test
{
    public class SampleObject
    {
        private int _number;
        
        [Refraction]
        public virtual int Number
        {
            get => _number;
            set => _number = value;
        }

        public SampleObject()
        {
            _number = -1;
        }

        public SampleObject(int number)
        {
            _number = number;
        }

        [Refraction]
        public virtual int AddNumber(int increment)
        {
            Number += increment;
            return Number;
        }
    }
}