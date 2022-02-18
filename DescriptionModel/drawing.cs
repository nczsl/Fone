using System;
using System.Collections.Generic;
using System.Text;

namespace DescriptionModel .drawing {
    public class D4Matrix {
        public float m11;
        public float m12;
        public float m13;
        public float m14;
        public float m24;
        public float m23;
        public float m22;
        public float m21;
        public float m31;
        public float m32;
        public float m33;
        public float m34;
        public float m44;
        public float m43;
        public float m42;
        public float m41;
        public D4Vector row1;
        public D4Vector row2;
        public D4Vector row3;
        public D4Vector row4;
        public D4Vector col4;
        public D4Vector col3;
        public D4Vector col2;
        public D4Vector col1;
    }
    public class D4Vector {
        public float x, y, z, a;
    }
    public class D2Vector {
        public float x, y;
    }
    public class D3Vector {
        public float x, y, z;
    }
    public class Effect {
        public D4Matrix world;
        public D4Matrix projection;
        public D4Matrix view;
    }
    public class  D3Obj{
    }
    public class D2Obj {

    }
}
