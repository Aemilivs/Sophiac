﻿using System;
using OpenAI_API;
using OpenAI_API.Chat;

namespace Sophiac.UI.Settings
{
	public class OpenAIAPIProvider
	{
		public string APIKey { get; set; } = string.Empty;

		public OpenAIAPIProvider(ISecureStorage storage)
		{
			APIKey = storage.GetAsync("openai-api-key").Result;
		}

		public async Task<string> PredictPayloadAsync(string input)
		{
			var api = new OpenAIAPI(APIKey);
			var conversation = api.Chat.CreateConversation();

            conversation.AppendSystemMessage("You are an AI agent generating questions from given text. The questions should be of three types: Single Choice, Multiple Choice, and Mapping Question. It's crucial that you produce a well-formatted and complete JSON output that is efficient in space usage, but without cutting off midway to save space. The resulting test set must contain at least 10 questions. Refer to the following example for the format. Generate only JSON payload and nothing more.");

            conversation.AppendUserInput("Chapter 3: Cell Structure and Function\n\nCells are the basic units of life and have specialized structures and functions that make them incredibly efficient. Each cell contains a variety of organelles, each having unique roles within the cell.\n\n    Mitochondrion: Often referred to as the powerhouse of the cell, the mitochondria are responsible for energy production. They generate a type of chemical energy called adenosine triphosphate (ATP), which fuels various biochemical reactions in the cell.\n\n    Nucleus: The nucleus acts as the control center of the cell. It houses most of the cell's genetic material, in the form of DNA (deoxyribonucleic acid). This DNA contains the instructions needed for building and maintaining the organism.\n\n    Ribosome: Ribosomes are the protein factories of the cell. They synthesize proteins according to the instructions given by the DNA. Proteins are essential for many cellular activities, including growth and repair.\n\n    Endoplasmic Reticulum: This is a network of tube-like structures within the cell, and it plays a significant role in both protein and lipid synthesis. There are two types of endoplasmic reticulum - rough and smooth. The rough endoplasmic reticulum, studded with ribosomes, is directly involved in protein synthesis. The smooth endoplasmic reticulum, on the other hand, focuses on lipid synthesis and also helps in detoxifying certain chemicals within the cell.\n\nIn summary, each organelle within the cell has a specific function. The mitochondria generate ATP for energy, the nucleus stores the cell's DNA, the ribosomes are responsible for protein synthesis, and the endoplasmic reticulum synthesizes proteins and lipids.\n\nRemember, understanding the function of each of these organelles is key to understanding how cells function as a whole. By mastering this information, you will be well on your way to understanding the fascinating world of cell biology.");
			conversation.AppendExampleChatbotOutput(@"{""Title"":""Cell Structure and Function Test"",""FileName"":""CellStructureAndFunctionTest.json"",""Questions"":[{""$type"":""Sophiac.Core.Questions.SingleChoiceQuestion, Sophiac.Core"",""Answers"":[{""$type"":""Sophiac.Core.Answers.SingleChoiceAnswer, Sophiac.Core"",""Content"":""Nucleus"",""Points"":0},{""$type"":""Sophiac.Core.Answers.SingleChoiceAnswer, Sophiac.Core"",""Content"":""Mitochondrion"",""Points"":10},{""$type"":""Sophiac.Core.Answers.SingleChoiceAnswer, Sophiac.Core"",""Content"":""Ribosome"",""Points"":0},{""$type"":""Sophiac.Core.Answers.SingleChoiceAnswer, Sophiac.Core"",""Content"":""Endoplasmic reticulum"",""Points"":0}],""Title"":""Which organelle is known as the powerhouse of the cell?"",""Description"":""Single Choice Question Description""},{""$type"":""Sophiac.Core.Questions.MultipleChoicesQuestion, Sophiac.Core"",""Answers"":[{""$type"":""Sophiac.Core.Answers.MultipleChoicesAnswer, Sophiac.Core"",""Content"":[{""Content"":""Ribosome""},{""Content"":""Endoplasmic reticulum""}],""Points"":10},{""$type"":""Sophiac.Core.Answers.MultipleChoicesAnswer, Sophiac.Core"",""Content"":[{""Content"":""Mitochondrion""},{""Content"":""Nucleus""}],""Points"":0}],""Title"":""Which of the following organelles are involved in protein synthesis?"",""Description"":""Multiple Choices Question Description""},{""$type"":""Sophiac.Core.Questions.MappingQuestion, Sophiac.Core"",""Answers"":[{""$type"":""Sophiac.Core.Answers.MappingAnswer, Sophiac.Core"",""Content"":[{""Source"":""Nucleus"",""Destination"":""Stores DNA""},{""Source"":""Mitochondrion"",""Destination"":""Generates ATP""},{""Source"":""Ribosome"",""Destination"":""Protein synthesis""},{""Content"":""Endoplasmic reticulum"",""Destination"":""Protein and lipid synthesis""}],""Points"":10}],""Title"":""Map the organelles to their functions."",""Description"":""Mapping Question Description""},{""$type"":""Sophiac.Core.Questions.SingleChoiceQuestion, Sophiac.Core"",""Answers"":[{""$type"":""Sophiac.Core.Answers.SingleChoiceAnswer, Sophiac.Core"",""Content"":""Cell wall"",""Points"":0},{""$type"":""Sophiac.Core.Answers.SingleChoiceAnswer, Sophiac.Core"",""Content"":""Nucleus"",""Points"":10},{""$type"":""Sophiac.Core.Answers.SingleChoiceAnswer, Sophiac.Core"",""Content"":""Mitochondrion"",""Points"":0},{""$type"":""Sophiac.Core.Answers.SingleChoiceAnswer, Sophiac.Core"",""Content"":""Ribosome"",""Points"":0}],""Title"":""Which organelle contains most of the cell's genetic material?"",""Description"":""Single Choice Question Description""},{""$type"":""Sophiac.Core.Questions.SingleChoiceQuestion, Sophiac.Core"",""Answers"":[{""$type"":""Sophiac.Core.Answers.SingleChoiceAnswer, Sophiac.Core"",""Content"":""Lysosome"",""Points"":0},{""$type"":""Sophiac.Core.Answers.SingleChoiceAnswer, Sophiac.Core"",""Content"":""Mitochondrion"",""Points"":0},{""$type"":""Sophiac.Core.Answers.SingleChoiceAnswer, Sophiac.Core"",""Content"":""Endoplasmic reticulum"",""Points"":10},{""$type"":""Sophiac.Core.Answers.SingleChoiceAnswer, Sophiac.Core"",""Content"":""Nucleus"",""Points"":0}],""Title"":""Which organelle is involved in lipid synthesis?"",""Description"":""Single Choice Question Description""},{""$type"":""Sophiac.Core.Questions.SingleChoiceQuestion, Sophiac.Core"",""Answers"":[{""$type"":""Sophiac.Core.Answers.SingleChoiceAnswer, Sophiac.Core"",""Content"":""Mitochondrion"",""Points"":0},{""$type"":""Sophiac.Core.Answers.SingleChoiceAnswer, Sophiac.Core"",""Content"":""Nucleus"",""Points"":0},{""$type"":""Sophiac.Core.Answers.SingleChoiceAnswer, Sophiac.Core"",""Content"":""Ribosome"",""Points"":10},{""$type"":""Sophiac.Core.Answers.SingleChoiceAnswer, Sophiac.Core"",""Content"":""Endoplasmic reticulum"",""Points"":0}],""Title"":""Which organelle is directly involved in protein synthesis?"",""Description"":""Single Choice Question Description""},{""$type"":""Sophiac.Core.Questions.SingleChoiceQuestion, Sophiac.Core"",""Answers"":[{""$type"":""Sophiac.Core.Answers.SingleChoiceAnswer, Sophiac.Core"",""Content"":""Cell wall"",""Points"":0},{""$type"":""Sophiac.Core.Answers.SingleChoiceAnswer, Sophiac.Core"",""Content"":""Nucleus"",""Points"":0},{""$type"":""Sophiac.Core.Answers.SingleChoiceAnswer, Sophiac.Core"",""Content"":""Mitochondrion"",""Points"":10},{""$type"":""Sophiac.Core.Answers.SingleChoiceAnswer, Sophiac.Core"",""Content"":""Ribosome"",""Points"":0}],""Title"":""Which organelle is responsible for generating ATP?"",""Description"":""Single Choice Question Description""}],""Strategy"":0}");

			conversation.AppendUserInput(input);

			return await conversation.GetResponseFromChatbotAsync();
		}
	}
}