﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Vb.Mongo.Engine.Examples.Data
{
    class Product
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public double Price { get; set; }
        public string CategoryCode { get; set; }
        public override string ToString()
        {
            return $"{{\n\tId:{Id}\n\tName:{Name}\n\tCode:{Code}\n\tPrice:{Price:F2}\n\tCategoryCode:{CategoryCode}\n}}";
        }
    }
}