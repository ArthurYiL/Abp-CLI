﻿using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using AbpTools.ProjectTemplates;
using AbpTools.Utils;
using McMaster.Extensions.CommandLineUtils;

namespace AbpTools.Commands
{
    [Command(Description = Consts.Descriptions.New.CommandDescription)]
    class NewCommand : AbpCommandBase
    {
        [Argument(0, nameof(Identifier), Consts.Descriptions.New.IdentifierDescription, ShowInHelpText = true)]
        [AllowedValues("console", "module", IgnoreCase = true)]
        public string Identifier { get; set; }

        [Option("-n|--name", Consts.Descriptions.New.NameDescription, CommandOptionType.SingleValue)]
        public string Name { get; set; }

        private Abp Parent { get; set; }

        public static List<string> IdentifierFolders =
            new List<string> {
                $".{Path.DirectorySeparatorChar}console",
                $".{Path.DirectorySeparatorChar}module"
            };

        public const string DefaultModuleNamePlaceholder = "AbpModuleName";

        protected override async Task<int> OnExecuteAsync(CommandLineApplication app)
        {
            Identifier = Identifier.ToLower();

            if (Name.IsNullOrWhiteSpace())
            {
                var defaultName = string.Empty;
                var promptText = string.Empty;
                switch (Identifier)
                {
                    case "console":
                        defaultName = Consts.DefaultConsoleName;
                        promptText = Consts.Descriptions.New.NameConsolePrompt;
                        break;
                    case "module":
                        defaultName = Consts.DefaultModuleName;
                        promptText = Consts.Descriptions.New.NameModulePrompt;
                        break;
                    default:
                        break;
                }

                Name = Prompt.GetString(promptText, defaultValue: defaultName);
            }

            var tplFinder = new TemplateFinder(TemplateName);

            var tplFilePath = await tplFinder.Fetch();

            var projectFolder = Path.Combine(Directory.GetCurrentDirectory(),
                $".{Path.DirectorySeparatorChar}src{Path.DirectorySeparatorChar}{Name}");
            if (!Directory.Exists(projectFolder))
            {
                Directory.CreateDirectory(projectFolder);
            }

            ExtractHelper.ExtractZipFile(tplFilePath, projectFolder, Identifier);

            if (Identifier == "module")
            {
                Placeholder = $"{Placeholder}.{DefaultModuleNamePlaceholder}";
            }

            RenameHelper.RenameFolders(projectFolder, Placeholder, Name, false, null);

            return 0;
        }

        public override List<string> CreateArgs()
        {
            var args = Parent.CreateArgs();
            args.Add("new");

            return args;
        }
    }
}
