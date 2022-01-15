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
        public int Number;

        public SampleObject()
        {
            Number = -1;
        }

        private int A = 3;
        
        public SampleObject(int number, int a, int b, int c)
        {
            Number = number;
        }

        public int Test(int a, int b, ProxyManager c, int d)
        {
            var u = (int)(TestParams(a, b, c, d, A));
            if (u == 3)
            {
                u = 2;
            }
            return u;
        }
        
        public object TestParams(params object[] arguments)
        {
            return 3;
        }

        [Refraction]
        public virtual int AddNumber(int increment)
        {
            Number += increment;
            return Number;
        }
    }
}