namespace WorkTrackPro.API.Models
{
    public class TaskItem
    {
        public int Id { get; set; }  //--//

        public string? Title { get; set; }   //--//

        public string? Description { get; set; }  //--//

        public int AssignedTo { get; set; }  //--//

        public string? Status { get; set; }   //--//

        public DateTime Date { get; set; }   //--//
    }
}
