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
            public int Country_report { get; set; }
            public int Country_newsletter { get; set; }
            public int Country_annual { get; set; }
        }

        public class My_Press_Tw
        {
            public int Press_SEQ { get; set; }       
            public string Press_Type { get; set; }
            public DateTime Press_Date { get; set; }
            public string Press_File { get; set; }
            public string Press_Title { get; set; }
            public string Press_Content { get; set; }
            public int Press_On { get; set; }
            public int Press_Prod_ID { get; set; }
            public string Press_Prod_Title { get; set; }
            public string Press_Prod_Category { get; set; }
            public int Press_Prod_Discon { get; set; }
            public string Press_Pub_Code { get; set; }
            public string Press_Pub_Name { get; set; }
            public DateTime Update { get; set; }
        }

        public class My_Product_Tw
        {
            public int Prod_ID { get; set; }
            public int Prod_Delete_Flg { get; set; }
            public string Prod_Title_EN { get; set; }
            public string Prod_Title_TW { get; set; }
            public string Prod_Type { get; set; }
            public string Prod_TypeCode { get; set; }
            public string Prod_MainCategory { get; set; }
            public string Prod_Category { get; set; }
            public string Prod_Topic { get; set; }
            public string Prod_SubTopic { get; set; }
            public string Prod_Country { get; set; }
            public string Prod_ID_Pub { get; set; }
            public string Prod_Pub_ID { get; set; }
            public string Prod_PubDate { get; set; }
            public string Prod_File { get; set; }
            public string Prod_SampleFile { get; set; }
            public int Prod_BrowseNG_Flg { get; set; }
            public int Prod_FullRepLink_Flg { get; set; }
            public string Prod_Intro_TW { get; set; }
            public DateTime Prod_Update { get; set; }
            public string Prod_CurrencyCode { get; set; }
            public float Prod_LowPrice { get; set; }
            public float Prod_BeforePrice { get; set; }
            public string Prod_ContentInfo { get; set; }
            public string Prod_Related_ID { get; set; }
            public string Prod_Related_Keywords { get; set; }
            public int Prod_IssueFrequency { get; set; }
            public int Prod_PrePub { get; set; }
            public int Prod_CoverImage { get; set; }
            public DateTime Prod_WhatsNew { get; set; }
            public int Prod_Link_EN { get; set; }
            public int Prod_Link_JP { get; set; }
            public int Prod_Link_KR { get; set; }
        }

        public class My_Publisher_Tw
        {
            public string Pub_ID { get; set; }
            public string Pub_Name { get; set; }
            public int Pub_Sample_Flg { get; set; }
            public int Pub_Browse_Flg { get; set; }
            public string Pub_Status { get; set; }
            public string Pub_ShippingRoute { get; set; }
            public int Pub_Organizer_Flg { get; set; }
            public int Pub_report { get; set; }
            public int Pub_annual { get; set; }
            public int Pub_newsletter { get; set; }
            public DateTime Pub_Update { get; set; }
            public string Pub_Intro_TW { get; set; }
            public string Pub_Name_TW { get; set; }
            public int Pub_Exclusive_TW { get; set; }
            public int Pub_EN { get; set; }
            public int Pub_KR { get; set; }
            public int Pub_JP { get; set; }
            public int Pub_TW { get; set; }
        }

        public class My_Region_Tw
        {
            public string Region_ID { get; set; }
            public string Region_Name_TW { get; set; }
            public int Region_report { get; set; }
            public int Region_annual { get; set; }
            public int Region_newsletter { get; set; }
        }

        public class My_Subtopic_Tw
        {
            public string SubTopic_ID { get; set; }
            public string SubTopic_Name_TW { get; set; }
            public string SubTopic_Intro_TW { get; set; }
            public string SubTopic_Topic_ID { get; set; }
            public int SubTopic_report { get; set; }
            public int SubTopic_newsletter { get; set; }
            public int SubTopic_annual { get; set; }
        }

        public class My_Topic_Tw
        {
            public string Topic_ID { get; set; }
            public string Topic_Name_TW { get; set; }
            public string Topic_Intro_TW { get; set; }
            public int Topic_DisplayFlag { get; set; }
            public int Topic_Sort_Order { get; set; }
            public int Topic_report { get; set; }
            public int Topic_newsletter { get; set; }
            public int Topic_annual { get; set; }
        }

        public class My_Event_Category_Tw
        {
            public string Code { get; set; }
            public string Name { get; set; }
            public string File { get; set; }
            public int Sort { get; set; }
            public int Count { get; set; }
        }

        public class My_Event_Press_Tw
        {
            public int Press_SEQ { get; set; }
            public string Press_Type { get; set; }
            public DateTime Press_Date { get; set; }
            public string Press_File { get; set; }
            public string Press_Title { get; set; }
            public string Press_Content { get; set; }
            public int Press_On { get; set; }
            public int Press_Prod_ID { get; set; }
            public string Press_Prod_Title { get; set; }
            public string Press_Prod_Category { get; set; }
            public int Press_Prod_Discon { get; set; }
            public string Press_Pub_Code { get; set; }
            public string Press_Pub_Name { get; set; }
            public DateTime Event_Start_Date { get; set; }
            public DateTime Event_End_Date { get; set; }
            public string Event_Venue { get; set; }
            public string Event_Country { get; set; }
            public int Prod_WEB_OnOff { get; set; }
            public string Prod_WEB_URL { get; set; }
            public int Prod_WEB_JP { get; set; }
            public int Prod_WEB_EN { get; set; }
            public int Prod_WEB_TW { get; set; }
            public int Prod_WEB_KR { get; set; }
            public DateTime Update { get; set; }
        }

        public class My_Event_Region_Tw
        {
            public string Code { get; set; }
            public string Name { get; set; }
            public int Count { get; set; }
            public int PCount { get; set; }
        }

        public class My_Partnar_Category_Tw
        {
            public string Code { get; set; }
            public string Name { get; set; }
            public string Title { get; set; }
            public string File { get; set; }
            public string WCon { get; set; }
            public int Count { get; set; }
            public int Sort { get; set; }

        }
    }
}
