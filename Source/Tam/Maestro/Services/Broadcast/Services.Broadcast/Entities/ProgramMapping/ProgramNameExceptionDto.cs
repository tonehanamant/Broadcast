namespace Services.Broadcast.Entities.ProgramMapping
{
	public class ProgramNameExceptionDto
	{
		public int Id { get; set; }
		public string CustomProgramName { get; set; }
		public int GenreId { get; set; }
		public string GenreName { get; set; }
		public int ShowTypeId { get; set; }
		public string ShowTypeName { get; set; }
	}
}