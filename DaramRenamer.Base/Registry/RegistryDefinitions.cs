using DaramRenamer.Commands;
using DaramRenamer.Conditions;
using DaramRenamer.Registry;

[assembly: CommandDefinition(typeof(ManualEditCommand), "manual.edit", "Command_Name_ManualEdit", "DaramRenamer.Commands.ManualEditCommand")]
[assembly: OptionDefinition(typeof(ManualEditCommand), nameof(ManualEditCommand.ChangeName), "Command_Argument_ManualEdit_ChangeName")]
[assembly: OptionDefinition(typeof(ManualEditCommand), nameof(ManualEditCommand.ChangePath), "Command_Argument_ManualEdit_ChangePath")]

[assembly: CommandDefinition(typeof(ReplacePlainCommand), "filename.replace_plain", "Command_Name_ReplacePlain", "DaramRenamer.Commands.Filename.ReplacePlainCommand")]
[assembly: OptionDefinition(typeof(ReplacePlainCommand), nameof(ReplacePlainCommand.Find), "Command_Argument_ReplacePlain_Find")]
[assembly: OptionDefinition(typeof(ReplacePlainCommand), nameof(ReplacePlainCommand.Replace), "Command_Argument_ReplacePlain_Replace")]
[assembly: OptionDefinition(typeof(ReplacePlainCommand), nameof(ReplacePlainCommand.IncludeExtension), "Command_Argument_ReplacePlain_IncludeExtension")]

[assembly: CommandDefinition(typeof(ReplaceRegexCommand), "filename.replace_regex", "Command_Name_ReplaceRegexp", "DaramRenamer.Commands.Filename.ReplaceRegexpCommand")]
[assembly: OptionDefinition(typeof(ReplaceRegexCommand), nameof(ReplaceRegexCommand.Find), "Command_Argument_ReplaceRegexp_Find")]
[assembly: OptionDefinition(typeof(ReplaceRegexCommand), nameof(ReplaceRegexCommand.Replace), "Command_Argument_ReplaceRegexp_Replace")]
[assembly: OptionDefinition(typeof(ReplaceRegexCommand), nameof(ReplaceRegexCommand.IncludeExtension), "Command_Argument_ReplaceRegexp_IncludeExtension")]

[assembly: CommandDefinition(typeof(ArrangeRegexCommand), "filename.arrange_regex", "Command_Name_RearrangeRegexp", "DaramRenamer.Commands.Filename.RearrangeRegexpCommand")]
[assembly: OptionDefinition(typeof(ArrangeRegexCommand), nameof(ArrangeRegexCommand.Regex), "Command_Argument_RearrangeRegexp_Find")]
[assembly: OptionDefinition(typeof(ArrangeRegexCommand), nameof(ArrangeRegexCommand.FormatString), "Command_Argument_RearrangeRegexp_Replace")]
[assembly: OptionDefinition(typeof(ArrangeRegexCommand), nameof(ArrangeRegexCommand.IncludeExtension), "Command_Argument_RearrangeRegexp_IncludeExtension")]

[assembly: CommandDefinition(typeof(ConcatCommand), "filename.concat", "Command_Name_Concatenate", "DaramRenamer.Commands.Filename.ConcatenateCommand")]
[assembly: OptionDefinition(typeof(ConcatCommand), nameof(ConcatCommand.Text), "Command_Argument_Concatenate_Text")]
[assembly: OptionDefinition(typeof(ConcatCommand), nameof(ConcatCommand.Position), "Command_Argument_Concatenate_Position")]
[assembly: OptionDefinition(typeof(ConcatCommand), nameof(ConcatCommand.IncludeExtension), "Command_Argument_Concatenate_IncludeExtension")]

[assembly: CommandDefinition(typeof(ConcatDirectoryCommand), "filename.concat_directory", "Command_Name_ConcatenateDirectoryName", "DaramRenamer.Commands.Filename.ConcatenateDirectoryNameCommand")]
[assembly: OptionDefinition(typeof(ConcatDirectoryCommand), nameof(ConcatDirectoryCommand.Position), "Command_Argument_ConcatenateDirectoryName_Position")]
[assembly: OptionDefinition(typeof(ConcatDirectoryCommand), nameof(ConcatDirectoryCommand.ApplyToDirectory), "Commamd_Argument_ConcatenateDirectoryName_ApplyToDirectory")]
[assembly: OptionDefinition(typeof(ConcatDirectoryCommand), nameof(ConcatDirectoryCommand.IncludeExtension), "Command_Argument_ConcatenateDirectoryName_IncludeExtension")]

[assembly: CommandDefinition(typeof(TrimCommand), "filename.trim", "Command_Name_Trim", "DaramRenamer.Commands.Filename.TrimCommand")]
[assembly: OptionDefinition(typeof(TrimCommand), nameof(TrimCommand.Position), "Command_Argument_Trim_Position")]

[assembly: CommandDefinition(typeof(DeleteBlockCommand), "filename.delete_block", "Command_Name_DeleteBlock", "DaramRenamer.Commands.Filename.DeleteBlockCommand")]
[assembly: OptionDefinition(typeof(DeleteBlockCommand), nameof(DeleteBlockCommand.StartBlock), "Command_Argument_DeleteBlock_StartBlock")]
[assembly: OptionDefinition(typeof(DeleteBlockCommand), nameof(DeleteBlockCommand.EndBlock), "Command_Argument_DeleteBlock_EndBlock")]
[assembly: OptionDefinition(typeof(DeleteBlockCommand), nameof(DeleteBlockCommand.DeleteAllBlocks), "Command_Argument_DeleteBlock_DeleteAllBlocks")]
[assembly: OptionDefinition(typeof(DeleteBlockCommand), nameof(DeleteBlockCommand.IncludeExtension), "Command_Argument_DeleteBlock_IncludeExtension")]

[assembly: CommandDefinition(typeof(DeleteCommand), "filename.delete", "Command_Name_DeleteFilename", "DaramRenamer.Commands.Filename.DeleteFilenameCommand")]

[assembly: CommandDefinition(typeof(SubstringCommand), "filename.substring", "Command_Name_Substring", "DaramRenamer.Commands.Filename.SubstringCommand")]
[assembly: OptionDefinition(typeof(SubstringCommand), nameof(SubstringCommand.StartIndex), "Command_Argument_Substring_StartIndex")]
[assembly: OptionDefinition(typeof(SubstringCommand), nameof(SubstringCommand.Length), "Command_Argument_Substring_Length")]
[assembly: OptionDefinition(typeof(SubstringCommand), nameof(SubstringCommand.IncludeExtension), "Command_Argument_Substring_IncludeExtension")]

[assembly: CommandDefinition(typeof(ConvensionCommand), "filename.convension", "Command_Name_Casecast", "DaramRenamer.Commands.Filename.CasecastCommand")]
[assembly: OptionDefinition(typeof(ConvensionCommand), nameof(ConvensionCommand.Convension), "Command_Argument_Casecast_Casecast")]

[assembly: CommandDefinition(typeof(AddExtensionCommand), "extension.add", "Command_Name_AddExtension", "DaramRenamer.Commands.Extension.AddExtensionCommand")]
[assembly: OptionDefinition(typeof(AddExtensionCommand), nameof(AddExtensionCommand.Extension), "Command_Argument_AddExtension_Extension")]
[assembly: OptionDefinition(typeof(AddExtensionCommand), nameof(AddExtensionCommand.ApplyToDirectory), "Commamd_Argument_AddExtension_ApplyToDirectory")]

[assembly: CommandDefinition(typeof(AddExtensionAutoCommand), "extension.add_auto", "Command_Name_AddExtensionAuto", "DaramRenamer.Commands.Extension.AddExtensionAutoCommand")]

[assembly: CommandDefinition(typeof(DeleteExtensionCommand), "extension.delete", "Command_Name_DeleteExtension", "DaramRenamer.Commands.Extension.DeleteExtensionCommand")]
[assembly: OptionDefinition(typeof(DeleteExtensionCommand), nameof(DeleteExtensionCommand.ApplyToDirectory), "Commamd_Argument_DeleteExtension_ApplyToDirectory")]

[assembly: CommandDefinition(typeof(ReplaceExtensionCommand), "extension.replace", "Command_Name_ReplaceExtension", "DaramRenamer.Commands.Extension.ReplaceExtensionCommand")]
[assembly: OptionDefinition(typeof(ReplaceExtensionCommand), nameof(ReplaceExtensionCommand.Extension), "Command_Argument_ReplaceExtension_Extension")]
[assembly: OptionDefinition(typeof(ReplaceExtensionCommand), nameof(ReplaceExtensionCommand.ApplyToDirectory), "Commamd_Argument_ReplaceExtension_ApplyToDirectory")]

[assembly: CommandDefinition(typeof(ConvensionExtensionCommand), "extension.convension", "Command_Name_CasecastExtension", "DaramRenamer.Commands.Extension.CasecastExtensionCommand")]
[assembly: OptionDefinition(typeof(ConvensionExtensionCommand), nameof(ConvensionExtensionCommand.Convension), "Command_Argument_CasecastExtension_Casecast")]
[assembly: OptionDefinition(typeof(ConvensionExtensionCommand), nameof(ConvensionExtensionCommand.ApplyToDirectory), "Commamd_Argument_CastcastExtension_ApplyToDirectory")]

[assembly: CommandDefinition(typeof(AbsolutePathCommand), "path.absolute", "Command_Name_AbsoluteGoTo", "DaramRenamer.Commands.FilePath.AbsoluteGoToCommand")]
[assembly: OptionDefinition(typeof(AbsolutePathCommand), nameof(AbsolutePathCommand.Path), "Command_Argument_AbsoluteGoTo_Path")]

[assembly: CommandDefinition(typeof(RelativePathCommand), "path.relative", "Command_Name_RelativeGoTo", "DaramRenamer.Commands.FilePath.RelativeGoToCommand")]
[assembly: OptionDefinition(typeof(RelativePathCommand), nameof(RelativePathCommand.Path), "Command_Argument_RelativeGoTo_Path")]

[assembly: CommandDefinition(typeof(AddIndexCommand), "number.add_index", "Command_Name_AddIndex", "DaramRenamer.Commands.Number.AddIndexCommand")]
[assembly: OptionDefinition(typeof(AddIndexCommand), nameof(AddIndexCommand.Position), "Command_Argument_AddIndex_Position")]

[assembly: CommandDefinition(typeof(DeleteNoNumberCommand), "number.delete_no_number", "Command_Name_DeleteNoNumber", "DaramRenamer.Commands.Number.DeleteNoNumberCommand")]
[assembly: OptionDefinition(typeof(DeleteNoNumberCommand), nameof(DeleteNoNumberCommand.Wordly), "Command_Argument_DeleteNoNumber_Wordly")]

[assembly: CommandDefinition(typeof(IncreaseCommand), "number.increase", "Command_Name_Increase", "DaramRenamer.Commands.Number.IncreaseCommand")]
[assembly: OptionDefinition(typeof(IncreaseCommand), nameof(IncreaseCommand.Count), "Command_Argument_Increase_Count")]
[assembly: OptionDefinition(typeof(IncreaseCommand), nameof(IncreaseCommand.Position), "Command_Argument_Increase_Position")]

[assembly: CommandDefinition(typeof(SameNumberCountCommand), "number.same_number_count", "Command_Name_SameNumberCount", "DaramRenamer.Commands.Number.SameNumberCountCommand")]
[assembly: OptionDefinition(typeof(SameNumberCountCommand), nameof(SameNumberCountCommand.Count), "Command_Argument_SameNumberCount_Count")]
[assembly: OptionDefinition(typeof(SameNumberCountCommand), nameof(SameNumberCountCommand.Position), "Command_Argument_SameNumberCount_Position")]

[assembly: CommandDefinition(typeof(AddDateCommand), "date.add", "Command_Name_AddDate", "DaramRenamer.Commands.Date.AddDateCommand")]
[assembly: OptionDefinition(typeof(AddDateCommand), nameof(AddDateCommand.Kind), "Command_Argument_AddDate_Kind")]
[assembly: OptionDefinition(typeof(AddDateCommand), nameof(AddDateCommand.Format), "Command_Argument_AddDate_Format")]
[assembly: OptionDefinition(typeof(AddDateCommand), nameof(AddDateCommand.Position), "Command_Argument_AddDate_Position")]

[assembly: CommandDefinition(typeof(DeleteDateCommand), "date.delete", "Command_Name_DeleteDate", "DaramRenamer.Commands.Date.DeleteDateCommand")]

[assembly: CommandDefinition(typeof(AddDocumentTagCommand), "tag.document", "Command_Name_AddDocumentTag", "DaramRenamer.Commands.Tags.AddDocumentTagCommand")]
[assembly: OptionDefinition(typeof(AddDocumentTagCommand), nameof(AddDocumentTagCommand.Tag), "Command_Argument_AddDocumentTag_Tag")]
[assembly: OptionDefinition(typeof(AddDocumentTagCommand), nameof(AddDocumentTagCommand.Position), "Command_Argument_AddDocumentTag_Position")]

[assembly: CommandDefinition(typeof(AddGitInfoCommand), "tag.git", "Command_Name_AddGitInfo", "DaramRenamer.Commands.Tags.AddGitInfoCommand")]
[assembly: OptionDefinition(typeof(AddGitInfoCommand), nameof(AddGitInfoCommand.GitInfo), "Command_Argument_AddGitInfo_GitInfo")]
[assembly: OptionDefinition(typeof(AddGitInfoCommand), nameof(AddGitInfoCommand.Position), "Command_Argument_AddGitInfo_Position")]

[assembly: CommandDefinition(typeof(AddHashCommand), "tag.hash", "Command_Name_AddHash", "DaramRenamer.Commands.Tags.AddHashCommand")]
[assembly: OptionDefinition(typeof(AddHashCommand), nameof(AddHashCommand.HashKind), "Command_Argument_AddHash_HashType")]
[assembly: OptionDefinition(typeof(AddHashCommand), nameof(AddHashCommand.Position), "Command_Argument_AddHash_Position")]

[assembly: CommandDefinition(typeof(AddMediaTagCommand), "tag.media", "Command_Name_AddMediaTag", "DaramRenamer.Commands.Tags.AddMediaTagCommand")]
[assembly: OptionDefinition(typeof(AddMediaTagCommand), nameof(AddMediaTagCommand.Tag), "Command_Argument_AddMediaTag_Tag")]
[assembly: OptionDefinition(typeof(AddMediaTagCommand), nameof(AddMediaTagCommand.Arguments), "Command_Argument_AddMediaTag_Arguments")]
[assembly: OptionDefinition(typeof(AddMediaTagCommand), nameof(AddMediaTagCommand.Position), "Command_Argument_AddMediaTag_Position")]

[assembly: CommandDefinition(typeof(AdvancedFormatCommand), "advanced.format", "Command_Name_AdvancedFormat", "DaramRenamer.Commands.AdvancedFormatCommand")]
[assembly: OptionDefinition(typeof(AdvancedFormatCommand), nameof(AdvancedFormatCommand.FileNameFormatString), "Command_Argument_AdvancedFormat_FileNameFormatString")]
[assembly: OptionDefinition(typeof(AdvancedFormatCommand), nameof(AdvancedFormatCommand.PathFormatString), "Command_Argument_AdvancedFormat_PathFormatString")]

[assembly: ConditionDefinition(typeof(TextFileCondition), "condition.text_file", "Condition_Name_TextFile", "DaramRenamer.Conditions.TextFileCondition")]
[assembly: ConditionDefinition(typeof(FileCondition), "condition.file", "Condition_IsFile", "DaramRenamer.Conditions.FileCondition")]
[assembly: ConditionDefinition(typeof(DirectoryCondition), "condition.directory", "Condition_IsDirectory", "DaramRenamer.Conditions.DirectoryCondition")]
[assembly: ConditionDefinition(typeof(ExtensionCondition), "condition.extension", "Condition_Name_Extension", "DaramRenamer.Conditions.ExtensionCondition")]
[assembly: OptionDefinition(typeof(ExtensionCondition), nameof(ExtensionCondition.Extension), "Condition_Argument_Extension_Extension")]
