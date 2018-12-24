using System;
using System.Collections.Generic;
using System.Text;

namespace TraditionalToSimplified
{
    public class Model
    {
        public class my_category_tw
        {
            public string Category_ID { get; set; }
            public string Category_Name_TW { get; set; }
        }

        public class Db
        {
            public string DbConnectString { get; set; }
        }
    }
}
