// Guids.cs
// MUST match guids.h
using System;

namespace Company.DataViewer
{
    static class GuidList
    {
        public const string guidDataViewerPkgString = "5d6fcbe4-d4be-494f-baaf-a73ee54ef81a";
        public const string guidDataViewerCmdSetString = "fbf64721-caf1-4dc1-a5c5-a965f7c3ddd6";
        public const string guidToolWindowPersistanceString = "348be800-cd67-4c35-be94-e77e8e7b25f8";

        public static readonly Guid guidDataViewerCmdSet = new Guid(guidDataViewerCmdSetString);
    };
}