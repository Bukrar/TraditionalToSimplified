using System;
using System.Collections.Generic;
using System.Text;

namespace TraditionalToSimplified
{
    public class Model
    {
        public class My_Category_Tw
        {
            public string Category_ID { get; set; }
            public string Category_Name_TW { get; set; }

            public string Category_File { get; set; }
            public int Category_Sort_Order { get; set; }
            public int Category_report { get; set; }
            public int Category_newsletter { get; set; }
            public int Category_annual { get; set; }
            public int Category_company { get; set; }
        }

        public class My_Country_Tw
        {
            public string Country_ID { get; set; }
            public string Country_Name_TW { get; set; }
        }

        public class My_Press_Tw
        {
            public string Press_SEQ { get; set; }
            public string Press_Title { get; set; }
        }

        public class My_Product_Tw
        {
            public string Prod_ID { get; set; }
            public string Prod_Title_TW { get; set; }
        }

        public class My_Publisher_Tw
        {
            public string Pub_ID { get; set; }
            public string Pub_Intro_TW { get; set; }
        }

        public class My_Region_Tw
        {
            public string Region_ID { get; set; }
            public string Region_Name_TW { get; set; }
        }

        public class My_Subtopic_Tw
        {
            public string SubTopic_ID { get; set; }
            public string SubTopic_Name_TW { get; set; }
        }

        public class My_Topic_Tw
        {
            public string Topic_ID { get; set; }
            public string Topic_Name_TW { get; set; }
            public string Topic_Intro_TW { get; set; }
        }

        public class My_Event_Category_Tw
        {
            public string Code { get; set; }
            public string Name { get; set; }
        }

        public class My_Event_Press_Tw
        {
            public string Press_SEQ { get; set; }
            public string Press_Title { get; set; }
            public string Press_Content { get; set; }
        }

        public class My_Event_Region_Tw
        {
            public string Code { get; set; }
            public string Name { get; set; }
        }

        public class My_Partnar_Category_Tw
        {
            public string Code { get; set; }
            public string Name { get; set; }
            public string Title { get; set; }
        }
    }
}
