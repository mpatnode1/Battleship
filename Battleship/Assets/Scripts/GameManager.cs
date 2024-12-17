using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

namespace Battleship
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField] private int[,] grid = new int[,]
        {
            //The top left is (0,0)
            {0, 0, 0, 0, 0},
            {0, 0, 0, 0, 0},
            {0, 0, 0, 0, 0},
            {0, 0, 0, 0, 0},
            {0, 0, 0, 0, 0}
            //Bottom right is 4, 4

        };

        //Represents where the player has fired
        private bool[,] hits;

        //Total rows and columns we have
        private int nRows;
        private int nCols;

        //Current row/column we are on
        private int row;
        private int col;

        //Correctly hit ships
        private int score;

        //Total time game has been running
        private int time;

        //Total number of ships
        private int shipCount = 3;

        //Parent of all cells
        [SerializeField] Transform gridRoot;

        //Template used to populate the grid
        [SerializeField] GameObject cellPrefab;
        [SerializeField] GameObject winLabel;
        [SerializeField] TextMeshProUGUI timeLabel;
        [SerializeField] TextMeshProUGUI scoreLabel;

        private void Awake()
        {
            //Initialize rows/cols to help us with our operations
            nRows = grid.GetLength(0);
            nCols = grid.GetLength(1);
            //Create identical 2D array to grid that is of the type bool instead of int
            hits = new bool[nRows, nCols];

            //Populate the grid using a loop
            //Needs to execute as many times to fill up the grid
            //Can figure this out by calculating rows + cols
            for (int i = 0; i < nRows * nCols; i++)
            {
                //Create an instance of the prefab and child it to the gridRoot
                Instantiate(cellPrefab, gridRoot);
            }

            setShipsUp(shipCount);

            //Shuffle();
            SelectCurrentCell();
            InvokeRepeating("IncrementTime", 1f, 1f);
        }


        /// <summary>
        /// Recursion thats called until all ships are placed
        /// </summary>
        /// <param name="shipCount">Number of Ships</param>
        /// <returns></returns>
        int setShipsUp(int shipCount)
        {
            
            //ship length is equal to the placement order
            //first ship placed is 3 units long
            //last is 1 unit
            placeShip(shipCount);
            Debug.Log("Current Ship: " + shipCount);

            //if all ships have been placed return
            if (shipCount == 1)
                return 0;
            else
                return setShipsUp(shipCount - 1);
        }

        void placeShip(int shipLength)
        {

                bool shipOrientation = isVertical();
                if (shipOrientation)
                {

                    //ship is vertical

                    //+1 to x because shipCount will always be at least 1
                    int x = Random.Range(0, 5);
                    int y = Random.Range(0, 6 - shipCount);

                        for (int i = 0; i < shipLength; i++)
                        {

                            grid[x, y] = 1;
                            y++;
                        }

                }
                else
                {
                    //ship is vertical

                    //+1 to x because shipCount will always be at least 1
                    int x = Random.Range(0, 6 - shipCount);
                    int y = Random.Range(0, 5);

                        for (int i = 0; i < shipLength; i++)
                        {
                            Debug.Log("Placing Point: " + x + "," + y);
                            grid[x, y] = 1;
                            x++;
                        }

                    
                
            }
        }

        

        /// <summary>
        /// Determines orientation of ship being placed
        /// </summary>
        /// <returns>true if vertical, false if horizontal</returns>
        bool isVertical()
        {
            return Random.value < 0.5f; ;
        }

        Transform GetCurrentCell()
        {
            //You can figure out the child index
            //of the cell that is a part of the grid
            //by calculating (rows*Cols) + col
            int index = (row * nCols) + col;

            //Return the child by index
            return gridRoot.GetChild(index);
        }

        void SelectCurrentCell()
        {
            //Get the current cell
            Transform cell = GetCurrentCell();
            //Set the "Cursor" image on
            Transform cursor = cell.Find("Cursor");
            cursor.gameObject.SetActive(true);
        }

        void UnselectCurrentCell()
        {
            //Get the current cell
            Transform cell = GetCurrentCell();
            //Set the "Cursor" image off
            Transform cursor = cell.Find("Cursor");
            cursor.gameObject.SetActive(false);
        }

        public void MoveHorizontal(int amt)
        {
            //Since we are moving to a new cell
            //we need to unselect the previous one
            UnselectCurrentCell();

            //Update the column
            col += amt;
            //Make sure the column stays within the bounds of the grid
            col = Mathf.Clamp(col, 0, nCols - 1);

            //Select the new cell
            SelectCurrentCell();
        }

        public void MoveVertical(int amt)
        {
            //Since we are moving to a new cell
            //we need to unselect the previous one
            UnselectCurrentCell();

            //Update the column
            row += amt;
            //Make sure the column stays within the bounds of the grid
            row = Mathf.Clamp(row, 0, nRows - 1);

            //Select the new cell
            SelectCurrentCell();
        }

        void ShowHit()
        {
            //Get the current cell
            Transform cell = GetCurrentCell();
            //Set the "Hit" image on
            Transform hit = cell.Find("Hit");
            hit.gameObject.SetActive(true);
        }

        void ShowMiss()
        {
            //Get the current cell
            Transform cell = GetCurrentCell();
            //Set the "Miss" image on
            Transform miss = cell.Find("Miss");
            miss.gameObject.SetActive(true);
        }

        void IncrementScore()
        {
            //Add 1 to the score
            score++;
            //Update the score label with current score
            scoreLabel.text = string.Format("Score: {0}", score);
        }

        public void Fire()
        {
            //Checks if the cell in the hits data is true or false
            //If it's true that means we already fired a shot in the current cell
            //And we should not do anything
            if (hits[row, col]) return;

            //Mark this cell as being fired upon
            hits[row, col] = true;

            //If this cell is a ship
            if (grid[row, col] == 1)
            {
                //Hit it
                //Increment score
                ShowHit();
                IncrementScore();
            }
            //If the cell is open water
            else
            {
                //Show miss
                ShowMiss();
            }

            TryEndGame();
        }

        void TryEndGame()
        {
            //Check every row
            for (int row = 0; row < nRows; row++)
            {
                //And check every column
                for (int col = 0; col < nCols; col++)
                {
                    //If a cell is not a ship then we can ignore
                    if (grid[row, col] == 0) continue;
                    //If a cell is a ship and it hasn't been scored
                    //then do a simple return because we haven't finished the game
                    if (hits[row, col] == false) return;
                }
            }

            //If the loop succesfully completes then all
            //ships have been destroyed and show the winLabel
            winLabel.SetActive(true);
            CancelInvoke("IncrementTime");
        }

        void IncrementTime()
        {
            //Add 1 to the time
            time++;
            //Update the time label with current time
            //Format it mm:ss where m is the minute and s is the seconds
            //ss should always display 2 digits
            //mm should only display as many digits that are necessary
            timeLabel.text = string.Format("{0}:{1}", time / 60, (time % 60).ToString("00"));
        }

        public void Restart()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    
}
