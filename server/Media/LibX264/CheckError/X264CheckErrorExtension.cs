namespace OptimeGBAServer.Media.LibX264.CheckError
{
    public static class X264CheckErrorExtension
    {
        public static int X264CheckError(this int ret)
        {
            if (ret < 0)
            {
                throw new X264Exception();
            }

            return ret;
        }
    }
}
