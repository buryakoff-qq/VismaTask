namespace VismaTask.Models;

public class Shortage
{
    public string Title { get; set; }
    public string Name { get; set; }
    public string Room { set; get; }
    public string Category { get; set; }
    public int Priority { get; set; }
    public DateTime CreatedOn { get; set; }

}
