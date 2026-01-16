using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Familiada.Models;

public  class Answer
{
    public string Text { get; set; } = string.Empty;
    public int Points { get; set; }
    public bool IsRevealed { get; set; } = false;
    public int Position { get; set; }
} 
