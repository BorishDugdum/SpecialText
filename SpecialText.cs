// Created by Warren Smith
// Used in Dark Flame (www.DarkFlameGame.com)
// Class used to move and change characters within SpriteFont

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Nobody_Cares
{
    public class SpecialText
    {
        private string currentText = "";
        private string fullText = "";
        private List<SpecialCharacters> specialCharacters;
        private SpriteFont font_16;
        private Vector2 dialogueOffset;
        private Random random;

        private int textMaxSpeed = 25;
        private int textSpeedCounter = 0;

        private bool languageFilter = true; //I pull this boolean from my Options Menu
        private bool alreadyFiltered = false;
        private bool doneWriting = false;

        float spacingX = 1f; 
        int portraitState = 0;

        /// <summary>
        /// Checks to see if the characters are done printing
        /// </summary>
        public bool DoneWriting
        { get { return doneWriting; } }

        public float GetSpacingX()
        { get { return spacingX; } }

        /// <summary>
        /// Load the content and properties for the object
        /// </summary>
        /// <param name="spriteFont">SpriteFont used</param>
        /// <param name="dialogueOffset">Offset on the screen to where the text will be displayed</param>
        /// <param name="rnd">Random</param>
        public void LoadContent(SpriteFont spriteFont, Vector2 dialogueOffset, Random rnd)
        {
            font_16 = spriteFont;
            specialCharacters = new List<SpecialCharacters>();
            this.dialogueOffset = dialogueOffset;
            random = rnd;
        }

        /// <summary>
        /// Reset all properties (prepare for next dialogue statement)
        /// </summary>
        private void ResetDialogue()
        {
            //Keep for now in case of change//
            languageFilter = OptionsManager.Instance.OptionsData.languageFilter;

            specialCharacters.Clear();
            currentText = "";
            fullText = "";
            alreadyFiltered = false;
            doneWriting = false;
            portraitState = 0;
            spacingX = 1f;
        }

        /// <summary>
        /// Use this method to create a new line of special characters to be drawn
        /// </summary>
        /// <param name="dialogueText">The actual string to be displayed</param>
        /// <param name="pState">The "Portrait State" of the character speaking - see switch statement for reference</param>
        /// <returns></returns>
        public string SetNewDialogue(string dialogueText, int pState)
        {
            if (fullText == dialogueText)
            {
                doneWriting = true;
                return fullText;
            }

            ResetDialogue();
            fullText = dialogueText;
            portraitState = pState;

            if (languageFilter && !alreadyFiltered)
                LanguageFilter(ref fullText);

            if (fullText == "")
            {
                doneWriting = true;
            }
            else
            {
                switch (portraitState)
                {
                    case 1: //Serious
                        textMaxSpeed = 10;
                        break;
                    case 2: //Angry
                        spacingX = 1.5f;
                        textMaxSpeed = 3;
                        break;
                    case 3: //Creepy
                        spacingX = 1.25f;
                        textMaxSpeed = 50;
                        break;
                    case 4: //Smirk
                        spacingX = 1.25f;
                        textMaxSpeed = 3;
                        break;
                    case 5: //Confused
                        break;
                    case 6: //Surprised
                        spacingX = 1.00f;
                        textMaxSpeed = 3;
                        break;
                    case 7: //Sad
                        spacingX = 1.25f;
                        textMaxSpeed = 75;
                        break;
                    default: //Normal
                        textMaxSpeed = 20;
                        break;
                } 
            }
            return fullText;
        }

        /// <summary>
        /// Used to print all the characters in the fullText at once (instead of individually).
        /// </summary>
        /// <param name="gameTime">Amount of time (in milliseconds) for each loop (~16 in XNA)</param>
        public void ForceFeedText(float gameTime)
        {
            int offsetIndex = 0; //number of times we multiply gameTime to offset moving Special Char positions
            for (int i = currentText.Count(); i < fullText.Count(); i++)
            {
                char newChar = fullText[currentText.Count()]; //Grab next character from full text//

                //Add a new character into the List while offsetting position based on how many characters are in array//
                specialCharacters.Add(new SpecialCharacters(
                    newChar, dialogueOffset + new Vector2(font_16.MeasureString(currentText).X * spacingX, font_16.MeasureString(fullText).Y * .5f),
                    random, portraitState, gameTime * offsetIndex, new Vector2(font_16.MeasureString("X").X, font_16.MeasureString("X").Y) / 2));
                
                currentText += newChar;
                offsetIndex++;
            }
            doneWriting = true;
        }

        /// <summary>
        /// Add/Update each SpecialCharacter in the fullText
        /// </summary>
        /// <param name="gameTime">Amount of time (in milliseconds) for each loop (~16 in XNA)</param>
        public void Update(float gameTime)
        {
            if (currentText != fullText)
            {
                if (textSpeedCounter >= textMaxSpeed)
                {
                    textSpeedCounter -= textMaxSpeed;
                    char newChar = fullText[currentText.Count()]; //Grab next character from full text//

                    //Add a new character into the List while offsetting position based on how many characters are in array//
                    specialCharacters.Add(new SpecialCharacters(
                        newChar, dialogueOffset + new Vector2(font_16.MeasureString(currentText).X * spacingX, font_16.MeasureString(fullText).Y * .5f),
                        random, portraitState, 0, new Vector2(font_16.MeasureString("X").X, font_16.MeasureString("X").Y) / 2));
                    
                    currentText += newChar;

                    //Quick check to see if text is full
                    if (currentText == fullText)
                        doneWriting = true;
                }
                else
                {
                    textSpeedCounter += (int)gameTime;
                }
            }

            foreach (SpecialCharacters sChars in specialCharacters)
            {
                sChars.Update(gameTime);
            }
        }

        /// <summary>
        /// Draw the SpecialCharacters to the screen
        /// </summary>
        /// <param name="spriteBatch">SpriteBatch used to draw (XNA/MonoGame)</param>
        public void Draw(SpriteBatch spriteBatch)
        {
            foreach (SpecialCharacters sChars in specialCharacters)
            {
                spriteBatch.DrawString(font_16, sChars.character.ToString(), sChars.Position, sChars.Color,
                        sChars.Rotation, sChars.Origin, sChars.Scale, sChars.Effects, 0);
            }
        }

        /// <summary>
        /// This is the private method to filter/change keywords
        /// </summary>
        /// <param name="textToFilter">Referenced text to filter</param>
        private void LanguageFilter(ref string textToFilter)
        {
            // These were only a few of the cusswords I used - I didn't put much effort into this part
            // This can be used for anything - You could change this to replace certain keywords
            // Or maybe even a full language (Like in Final Fantasy X)
            textToFilter = textToFilter.Replace("Damn", "!@%#");
            textToFilter = textToFilter.Replace("damn", "!@%#");
            textToFilter = textToFilter.Replace("Fuck", "@$#%");
            textToFilter = textToFilter.Replace("fuck", "@$#%");
            textToFilter = textToFilter.Replace("Shit", "$#^&");
            textToFilter = textToFilter.Replace("shit", "$#^&");
            textToFilter = textToFilter.Replace("Ass", "Butt");
            textToFilter = textToFilter.Replace(" ass", " butt");
            textToFilter = textToFilter.Replace("Hell", "Heck");
            textToFilter = textToFilter.Replace("hell", "heck");

            alreadyFiltered = true;
        }
    }


    //Special char class used for the text above
    class SpecialCharacters
    {
        //Static Displacement - Relation to Screen//
        static Vector2 titleSafeArea = new Vector2(Camera.Instance.TitleSafeArea.X, Camera.Instance.TitleSafeArea.Y);
        static Vector2 displacement = new Vector2(40, 120);
        static Vector2 nameDisplacement = new Vector2(40, 8);
        static Vector2 dialogueDisplacement = new Vector2(140, 40);

        //Primary Variables//
        public Vector2 Position = Vector2.Zero;
        public char character = '?';
        private Vector2 originalPosition = Vector2.Zero;
        private float moveRadius = 2;
        private Vector2 moveCounter = Vector2.Zero; //randomize this for disorient effect//
        private Vector2 moveSpeed = new Vector2(0, .5f);
        public Color Color = Color.Wheat;

        //Secondary Variables//
        public float Rotation = 0f;
        public Vector2 Origin = Vector2.Zero;
        public float Scale = 1f;
        public SpriteEffects Effects = SpriteEffects.None;
        private bool continuousMovement = true;
        

        public SpecialCharacters(char c, Vector2 charDisplacement, Random rnd, int portraitState, float moveCounterOffset, Vector2 origin)
        {
            character = c;
            Origin = origin;
            originalPosition = Position = titleSafeArea + displacement + dialogueDisplacement + charDisplacement - origin;
            continuousMovement = true;

            switch (portraitState)
            {
                case 1: //Serious
                    moveSpeed = new Vector2(4f, 0);
                    Color = new Color(225, 185, 100);
                    moveRadius = 20;
                    continuousMovement = false;
                    break;
                case 2: //Angry
                    moveCounter.Y = rnd.Next(0, 100) * .01f * (float)Math.PI;
                    moveSpeed = new Vector2(rnd.Next(50,100) * .1f, rnd.Next(50,150) * .1f);
                    moveRadius = 2;
                    Color = new Color(225, 105, 50); //Color.Orange;
                    break;
                case 3: //Creepy
                    moveCounter.Y = rnd.Next(0, 100) * .01f * (float)Math.PI;
                    moveSpeed = new Vector2(rnd.Next(10,11) * .1f, rnd.Next(10,11) * .1f);
                    moveRadius = 2;
                    Color = new Color(89, 117, 112); //PaleGreen;
                    break;
                case 4: //Smirk
                    moveSpeed = new Vector2(0, 1f);
                    Color = new Color(245, 245, 100); //Color.GreenYellow;
                    moveRadius = 20;
                    continuousMovement = false;
                    break;
                case 5: //Confused
                    moveSpeed = new Vector2(rnd.Next(12, 13) * .1f, rnd.Next(12, 13) * .1f);
                    Color = new Color(128, 70, 128);//Color.MediumPurple;
                    moveRadius = 10;
                    continuousMovement = false;
                    break;
                case 6: //Suprised
                    moveSpeed = new Vector2(0, 1f);
                    Color = new Color(245, 245, 100); //Color.GreenYellow;
                    moveRadius = 20;
                    continuousMovement = false;
                    break;
                case 7: //Sad
                    moveCounter.Y = rnd.Next(0, 100) * .01f * (float)Math.PI;
                    moveSpeed = new Vector2(rnd.Next(15, 20) * .1f, rnd.Next(10, 15) * .1f);
                    Color = new Color(195, 175, 225);//Color.MediumPurple;
                    moveRadius = 4;
                    break;
                default: //Regular
                    Color = Color.Wheat;
                    moveSpeed = Vector2.Zero;
                    break;

            }

            if (moveSpeed.X != 0)
            {
                moveCounter.X += (moveSpeed.X * moveCounterOffset * .01f);
                while (moveCounter.X > (float)Math.PI * 2)
                {
                    moveCounter.X -= (float)Math.PI * 2;
                }
            }
            if (moveSpeed.Y != 0)
            {
                moveCounter.Y += (moveSpeed.Y * moveCounterOffset * .01f);
                while (moveCounter.Y > (float)Math.PI * 2)
                {
                    moveCounter.Y -= (float)Math.PI * 2;
                }
            }
        }

        public void Update(float gameTime)
        {
            //Move Y Axis//
            moveCounter.Y += moveSpeed.Y * gameTime * .01f;
            Position.Y = (originalPosition.Y) + ((moveRadius) * (float)((Math.Sin(moveCounter.Y)) / 2));
            if (moveCounter.Y >= (float)Math.PI * 2)
            {
                moveCounter.Y -= (float)Math.PI * 2;
            }
            if (!continuousMovement)
            {
                if (moveCounter.Y >= (float)Math.PI)
                {
                    moveCounter.Y = (float)Math.PI;
                    Position.Y = (originalPosition.Y) + ((moveRadius) * (float)((Math.Sin(moveCounter.Y)) / 2));
                }

                if (moveCounter.X >= (float)Math.PI)
                {
                    moveCounter.X = (float)Math.PI;
                    Position.X = (originalPosition.X) + ((moveRadius / 2) * (float)((Math.Sin(moveCounter.X)) / 2));
                }
            }

            //Move X Axis by half the amount//
            moveCounter.X += moveSpeed.X * gameTime * .01f;
            Position.X = (originalPosition.X) + ((moveRadius / 2) * (float)((Math.Sin(moveCounter.X)) / 2));
            if (moveCounter.X >= (float)Math.PI * 2)
            {
                moveCounter.X -= (float)Math.PI * 2;
            }

            //Clamp the Position to Points//
            Position = new Vector2((int)Math.Round(Position.X), (int)Math.Round(Position.Y));
        }
    }
}
