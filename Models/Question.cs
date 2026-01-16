using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Familiada.Models;

public class Question
{
    public int Id { get; set; } = 0;
    public string Text { get; set; } =   string.Empty;
    public List<Answer> Answers { get; set; } = null!;
}