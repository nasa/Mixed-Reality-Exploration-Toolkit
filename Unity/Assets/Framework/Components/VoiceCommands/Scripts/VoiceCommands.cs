// Copyright © 2018-2021 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Windows.Speech;

namespace GSFC.ARVR.MRET.Infrastructure.Components.VoiceCommanding
{
    /// <remarks>
    /// History:
    /// 10 March 2021: Refactored
    /// </remarks>
    /// <summary>
    /// This script handles voice command recognition.
    /// Author: Christopher Trombley
    /// Refactored by Dylan Z. Baker
    /// </summary>
    public class VoiceCommands : MRETBehaviour
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName
        {
            get
            {
                return nameof(VoiceCommands);
            }
        }

        /// <summary>
        /// Class for a voice mapping.
        /// </summary>
        [System.Serializable]
        public class VoiceMapping
        {
            public string key;
            public VoiceCommandMapping.RecognitionType recognitionType;

            public VoiceMapping(string keyToSet, VoiceCommandMapping.RecognitionType recognitionTypeToSet)
            {
                key = keyToSet;
                recognitionType = recognitionTypeToSet;
            }
        }

        /// <summary>
        /// Class for a voice command mapping.
        /// </summary>
        [System.Serializable]
        public class VoiceCommandMapping
        {
            public enum RecognitionType { Keyword, Phrase }

            public List<VoiceMapping> commands;
            public UnityEvent onCall;

            public VoiceCommandMapping(List<VoiceMapping> commandsToSet,
                UnityEvent onCallToSet)
            {
                commands = commandsToSet;
                onCall = onCallToSet;
            }
        }

        /// <summary>
        /// Dictionary to map spoken text to command.
        /// </summary>
        [Tooltip("Dictionary to map spoken text to command.")]
        public List<VoiceCommandMapping> commandMappings = new List<VoiceCommandMapping>();

        /// <summary>
        /// Confidence level to use for command mappings.
        /// </summary>
        [Tooltip("Confidence level to use for command mappings.")]
        public ConfidenceLevel confidenceLevel = ConfidenceLevel.Low;

        /// <summary>
        /// The keyword reconizer object.
        /// </summary>
        private KeywordRecognizer keywordRecognizer = null;

        /// <summary>
        /// The dictation recognizer object.
        /// </summary>
        private DictationRecognizer dictationRecognizer = null;

        protected override void MRETStart()
        {
            base.MRETStart();

            if (Framework.MRET.ConfigurationManager.config.VoiceControl == true)
            {
                StartListening();
            }
        }

        /// <summary>
        /// Start listening for voice input.
        /// </summary>
        public void StartListening()
        {
            if (StartPhraseRecognition() == false)
            {
                FallbackToKeywordRecognition();
            }
        }

        /// <summary>
        /// Stop listening to voice input.
        /// </summary>
        public void StopListening()
        {
            StopPhraseRecognition();
            StopKeywordRecognition();
        }

        /// <summary>
        /// Start phrase recognition.
        /// </summary>
        /// <returns>True if phrase recognition is started, false if not.</returns>
        private bool StartPhraseRecognition()
        {
            dictationRecognizer = new DictationRecognizer();
            dictationRecognizer.DictationHypothesis += OnPhraseHypothesis;
            dictationRecognizer.DictationResult += OnPhraseResult;
            dictationRecognizer.DictationComplete += OnPhraseComplete;
            dictationRecognizer.DictationError += OnPhraseError;
            dictationRecognizer.Start();

            return true;
        }

        /// <summary>
        /// Stop phrase recognition.
        /// </summary>
        private void StopPhraseRecognition()
        {
            if (dictationRecognizer != null)
            {
                dictationRecognizer.Stop();
                dictationRecognizer.Dispose();
            }
        }

        /// <summary>
        /// Restart phrase recognition.
        /// </summary>
        private void RestartPhraseRecognition()
        {
            StopPhraseRecognition();
            StartPhraseRecognition();
        }

        /// <summary>
        /// Start keyword recognition.
        /// </summary>
        /// <returns>True if keyword recognition is started, false if not.</returns>
        private bool StartKeywordRecognition()
        {
            List<string> words = new List<string>();
            foreach (VoiceCommandMapping commandMapping in commandMappings)
            {
                if (commandMapping != null)
                {
                    foreach (VoiceMapping command in commandMapping.commands)
                    {
                        if (command.recognitionType == VoiceCommandMapping.RecognitionType.Keyword)
                        {
                            words.Add(command.key);
                        }
                    }
                }
            }

            keywordRecognizer = new KeywordRecognizer(words.ToArray(), confidenceLevel);
            keywordRecognizer.OnPhraseRecognized += OnKeywordRecognized;
            keywordRecognizer.Start();

            return true;
        }

        /// <summary>
        /// Stop keyword recognition.
        /// </summary>
        private void StopKeywordRecognition()
        {
            if (keywordRecognizer != null)
            {
                keywordRecognizer.Stop();
                keywordRecognizer.Dispose();
            }

            keywordRecognizer = null;
        }

        /// <summary>
        /// Restart keyword recognition.
        /// </summary>
        private void RestartKeywordRecognition()
        {
            StopKeywordRecognition();
            StartKeywordRecognition();
        }

        /// <summary>
        /// Fall back to keyword recognition after
        /// phrase recognition has failed.
        /// </summary>
        private void FallbackToKeywordRecognition()
        {
            Debug.Log("[VoiceCommand] Phrase recognition failed, falling back to keyword recognition.");
            StopPhraseRecognition();
            StartKeywordRecognition();
        }

        /// <summary>
        /// Called when a keyword is recognized.
        /// </summary>
        /// <param name="args">Keyword that is recognized.</param>
        private void OnKeywordRecognized(PhraseRecognizedEventArgs args)
        {
            CommandReceived(args.text, VoiceCommandMapping.RecognitionType.Keyword);
        }

        /// <summary>
        /// Called when a phrase hypothesis is found.
        /// </summary>
        /// <param name="command">Phrase hypothesis that is found.</param>
        private void OnPhraseHypothesis(string command)
        {
            CommandReceived(command, VoiceCommandMapping.RecognitionType.Phrase);
        }

        /// <summary>
        /// Called when phrase recognition is complete.
        /// </summary>
        /// <param name="cause">Cause of the completion.</param>
        private void OnPhraseComplete(DictationCompletionCause cause)
        {
            switch (cause)
            {
                case DictationCompletionCause.TimeoutExceeded:
                case DictationCompletionCause.PauseLimitExceeded:
                case DictationCompletionCause.Canceled:
                case DictationCompletionCause.Complete:
                    RestartPhraseRecognition();
                    break;

                case DictationCompletionCause.UnknownError:
                case DictationCompletionCause.AudioQualityFailure:
                case DictationCompletionCause.MicrophoneUnavailable:
                case DictationCompletionCause.NetworkFailure:
                    FallbackToKeywordRecognition();
                    break;
            }
        }

        /// <summary>
        /// Called when a phrase result is found.
        /// </summary>
        /// <param name="command">Command that is found.</param>
        /// <param name="confidenceLevel">Confidence in match.</param>
        private void OnPhraseResult(string command, ConfidenceLevel confidenceLevel)
        {
            CommandReceived(command, VoiceCommandMapping.RecognitionType.Phrase);
        }

        /// <summary>
        /// Called when a phrase error occurs.
        /// </summary>
        /// <param name="error">Error message.</param>
        /// <param name="hresult">Result code.</param>
        private void OnPhraseError(string error, int hresult)
        {
            FallbackToKeywordRecognition();
        }

        /// <summary>
        /// Handle reception of a command.
        /// </summary>
        /// <param name="command">Command string.</param>
        /// <param name="type">Recognition type used to perform match.</param>
        private void CommandReceived(string command, VoiceCommandMapping.RecognitionType type)
        {
            UnityEvent commandEvent = GetMapping(command, type);
            if (commandEvent != null)
            {
                commandEvent.Invoke();
                 Debug.Log("[Voice command invoked] " + command);
            }
        }

        /// <summary>
        /// Finds the Unity event associated with a command and recognition type.
        /// </summary>
        /// <param name="command">The command in question.</param>
        /// <param name="recognitionType">The recognition type in question.</param>
        /// <returns>The Unity event.</returns>
        private UnityEvent GetMapping(string command, VoiceCommandMapping.RecognitionType recognitionType)
        {
            foreach (VoiceCommandMapping commandMapping in commandMappings)
            {
                VoiceMapping foundCommand = commandMapping.commands.Find(x => x.key == command);
                if (foundCommand != null)
                {
                    if (foundCommand.recognitionType == recognitionType)
                    {
                        return commandMapping.onCall;
                    }
                }
            }

            return null;
        }
    }
}