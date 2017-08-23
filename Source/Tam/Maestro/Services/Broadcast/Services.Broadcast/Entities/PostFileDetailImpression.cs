namespace Services.Broadcast.Entities
{
    public class PostFileDetailImpression
    {
        public readonly int Demo;
        public readonly double Impression;
        public readonly int FileDetailId;

        public PostFileDetailImpression(int demo, double impression, int fileDetailId)
        {
            Demo = demo;
            Impression = impression;
            FileDetailId = fileDetailId;
        }
    }
}