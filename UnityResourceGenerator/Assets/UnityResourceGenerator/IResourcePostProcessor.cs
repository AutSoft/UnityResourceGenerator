﻿namespace UnityResourceGenerator
{
    public interface IResourcePostProcessor
    {
        int PostProcessPriority { get; }
        string PostProcess(ResourceContext context, string resourceFileContent);
    }
}