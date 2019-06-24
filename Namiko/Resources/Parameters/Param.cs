﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Namiko
{
    public class Param
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Args { get; set; }
        public ulong Num { get; set; }
        public DateTime Date { get; set; }
    }
}
