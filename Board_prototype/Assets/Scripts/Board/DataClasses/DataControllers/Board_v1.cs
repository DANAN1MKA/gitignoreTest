﻿using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class Board_v1 : BoardFather
{
    [Inject] private IBoardTimeController timer;
    [SerializeField] float time;
    [SerializeField] float additionalTime;

    private bool isItFirstMatch = false; //проверка на первый матч

    [Inject] private IMoveElementsManager moveController;

    [Inject] private IElementGenerator elementGenerator;

    private Element[,] board;

    private List<Element> foundMatches;

    void Start()
    {
        board = elementGenerator.generateBoard(width, heigth, _thisTransform);

        foundMatches = new List<Element>();

        //сообщаем прогрес бару сколько секунд мы отсчитываем
        timer.setConfigProgresBar(time);
    }

    private bool isItMatch(Element element)
    {
        List<Element> matchElementsX = new List<Element>();
        List<Element> matchElementsY = new List<Element>();
        matchElementsX.Add(element);
        matchElementsY.Add(element);

        //смотрим матчи по горизонтали
        Element neighbourLeft = element; bool leftIsDone = false;
        Element neighbourRight = element; bool rightIsDone = false;

        do
        {
            if (neighbourLeft.posX > 0) neighbourLeft = board[neighbourLeft.posX - 1, neighbourLeft.posY];
            else leftIsDone = true;

            if (neighbourRight.posX < width - 1) neighbourRight = board[neighbourRight.posX + 1, neighbourRight.posY];
            else rightIsDone = true;
            //else break;

            if (!leftIsDone && neighbourLeft.type == element.type) matchElementsX.Add(neighbourLeft); else leftIsDone = true;
            if (!rightIsDone && neighbourRight.type == element.type) matchElementsX.Add(neighbourRight); else rightIsDone = true;
        }
        while (!leftIsDone || !rightIsDone);

        //смотрим матчи по вертикали
        Element neighbourDown = element; bool DownIsDone = false;
        Element neighbourUp = element; bool UpIsDone = false;

        do
        {
            if (neighbourDown.posY > 0) neighbourDown = board[neighbourDown.posX, neighbourDown.posY - 1];
            else DownIsDone = true;

            if (neighbourUp.posY < heigth - 1) neighbourUp = board[neighbourUp.posX, neighbourUp.posY + 1];
            else UpIsDone = true;
            //else break;

            if (!DownIsDone && neighbourDown.type == element.type) matchElementsY.Add(neighbourDown); else DownIsDone = true;
            if (!UpIsDone && neighbourUp.type == element.type) matchElementsY.Add(neighbourUp); else UpIsDone = true;
        }
        while (!UpIsDone || !DownIsDone);

        //Если найдены матчи возвращаем правду иначе ложь
        if (matchElementsX.Count > 2 || matchElementsY.Count > 2)
        {
            //Запоминем найденные матчи
            if (matchElementsX.Count > 2)
            {
                addMatches(matchElementsX);
            }
            if (matchElementsY.Count > 2)
            {
                addMatches(matchElementsY);
            }

            return true;
        }
        else return false;
    }

    private void addMatches(List<Element> _from)
    {
        foreach (Element elem in _from)
        {
            if (!foundMatches.Contains(elem))
            {
                foundMatches.Add(elem);

                elem.block();
            }
        }
    }

    public override bool grabElement(int _x, int _y)
    {
        if (!board[_x, _y].getState())
        {
            board[_x, _y].block();
            return true;
        }
        else return false;
    }

    public override void swipeElement(int _x, int _y, Vector2 _direction)
    {
        if (_direction.x != 0 || _direction.y != 0)
        {
            if (!board[_x + (int)_direction.x, _y + (int)_direction.y].getState())
            {
                swipe(board[_x, _y], board[_x + (int)_direction.x, _y + (int)_direction.y]);

                bool match1 = isItMatch(board[_x, _y]);
                bool match2 = isItMatch(board[_x + (int)_direction.x, _y + (int)_direction.y]);

                if (match1 || match2)
                {
                    if (match1) board[_x, _y].block(); else board[_x, _y].unblock();

                    if (match2) board[_x + (int)_direction.x, _y + (int)_direction.y].block(); 
                    else board[_x + (int)_direction.x, _y + (int)_direction.y].unblock();


                    Vector2 element1 = new Vector2(_thisTransform.position.x + board[_x, _y].posX,
                                                   _thisTransform.position.y + board[_x, _y].posY);

                    Vector2 element2 = new Vector2(_thisTransform.position.x + board[_x + (int)_direction.x, _y + (int)_direction.y].posX,
                                                   _thisTransform.position.y + board[_x + (int)_direction.x, _y + (int)_direction.y].posY);

                    moveController.addElement(new MovingElement(board[_x, _y], element1),
                                              new MovingElement(board[_x + (int)_direction.x, _y + (int)_direction.y], element2));

                    if (!isItFirstMatch)
                    {
                        timer.setTimer(time);
                        isItFirstMatch = true;
                    }
                    else timer.setTimer(additionalTime);

                }
                else
                {
                    board[_x, _y].unblock();
                    board[_x + (int)_direction.x, _y + (int)_direction.y].unblock();

                    swipe(board[_x, _y], board[_x + (int)_direction.x, _y + (int)_direction.y]);
                }
            }
            else board[_x, _y].unblock();
        }
        else board[_x, _y].unblock();
    }

    private void swipe(Element element1, Element element2)
    {
        Element tmp = element1.getElement();
        element1.setElement(element2);
        element2.setElement(tmp);
    }

    public override void timerHandler()
    {
        isBlocked = true;
        isItFirstMatch = false;

        foundMatchesHandler();
    }

    public override void animationCompleted()
    {
        if (isBlocked)
        {
            if (foundMatches.Count > 0)
            {
                foundMatchesHandler();
            }
            else isBlocked = false;
        }
    }

    private void matchCascad()
    {

        /*
         на поле проверяем только элементы помеченые единицой
         * 1 * * 1 * *
         1 * * 1 * * 1
         * * 1 * * 1 *
         * 1 * * 1 * *
         1 * * 1 * * 1
         * * 1 * * 1 *
         * 1 * * 1 * *
         1 * * 1 * * 1
         */
        int shiftCounter = 0;
        for (int j = 0; j < heigth; j++)
        {
            for (int i = shiftCounter; i < width; i += 3)
            {
                isItMatch(board[i, j]);
            }
            shiftCounter = shiftCounter < 2 ? shiftCounter + 1 : 0;
        }
    }

    private void foundMatchesHandler()
    {
        foundMatches.Clear();

        List<MovingElement> fallingElements = new List<MovingElement>();

        int[] countForColumn = new int[width];

        for (int i = 0; i < heigth; i++)
        {
            for (int j = 0; j < width; j++)
            {
                //если элемент в матче 
                if (board[j, i].getState())
                {

                    elementGenerator.changeTypeCommon(board[j, i]);
                    countForColumn[j]++;


                    int count = board[j, i].posY;

                    while (count < heigth - 1 && board[j, count].getState()) count++;

                    if (!board[j, count].getState())
                    {
                        swipe(board[j, i], board[j, count]);

                        board[j, i].unblock();
                        board[j, count].block();

                    }
                }
            }
        }

        countForColumn = new int[width];

        for (int i = 0; i < heigth; i++)
        {
            for (int j = 0; j < width; j++)
            {
                fallingElements.Add(new MovingElement(
                    board[j, i],
                    new Vector2(_thisTransform.position.x + board[j, i].posX,
                                _thisTransform.position.y + board[j, i].posY)));

                if (board[j, i].getState())
                {
                    board[j, i].piece.transform.position = new Vector2(_thisTransform.position.x + board[j, i].posX, _thisTransform.position.y + heigth + countForColumn[j]);

                    board[j, i].unblock();
                    countForColumn[j]++;
                }

                board[j, i].resetAnimanion();
            }
        }

        moveController.dropElements(fallingElements);

        matchCascad();
    }

}
