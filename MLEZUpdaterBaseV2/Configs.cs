using System;
using System.Collections.Generic;
using System.Text;

namespace MLEZUpdaterBase
{
    public class il2cppdumpConfig
    {
        public bool DumpMethod = true;
        public bool DumpField = true;
        public bool DumpProperty = true;
        public bool DumpAttribute = true;
        public bool DumpFieldOffset = true;
        public bool DumpMethodOffset = true;
        public bool DumpTypeDefIndex = true;
        public bool DummyDll = true;
        public bool MakeFunction = true;
        public bool RequireAnyKey = false;
        public bool ForceIl2CppVersion = false;
    }
}
