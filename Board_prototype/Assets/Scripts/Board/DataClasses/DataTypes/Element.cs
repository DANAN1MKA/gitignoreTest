﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Element
{
    private float speed = 6f;

    public int type { get; set; } //определяет тип элемента: 0 - топор
                                  //                         1 - меч
                                  //                         2 - лук
                                  //                         3 - зелье итд.
                                  //                        -1 - уничтожен

    //позиция элемента на доске
    public int posX { get; set; }
    public int posY { get; set; }

    private bool isBlocked { get; set; } // элемент не должен двигаться если попал в матч 

    public GameObject piece;
    public SpriteRenderer spriteRenderer;
    public Animator animator;

    public Element(GameObject _piece)
    {
        setPiece(_piece);
    }

    private Element(GameObject _element, SpriteRenderer _spriteRenderer, Animator _animator)
    {
        piece = _element;
        spriteRenderer = _spriteRenderer;
        animator = _animator;
    }

    public void setPiece(GameObject _piece)
    {
        piece = _piece;
        spriteRenderer = piece.GetComponent<SpriteRenderer>();
        animator = piece.GetComponent<Animator>();
    }

    public void setElement(Element elem)
    {
        piece = elem.piece;
        type = elem.type;
        spriteRenderer = elem.spriteRenderer;

        animator = elem.animator;
    }

    public void moveSoft(Vector2 direction)
    {
        piece.transform.position = Vector2.Lerp(piece.transform.position, direction, Time.deltaTime * speed);
    }

    public void moveHard(Vector2 move)
    {
        piece.transform.position = Vector2.MoveTowards(piece.transform.position, move, Time.deltaTime * speed);
    }

    public Element getElement()
    {
        Element clone = new Element(this.piece, this.spriteRenderer, this.animator);
        clone.type = this.type;

        return clone;
    }

    public void block()
    {
        animator.StartPlayback();
        animator.Play(0);
        isBlocked = true;
    }

    public void unblock()
    {
        animator.StopPlayback();

        isBlocked = false;
    }

    public bool getState() { return isBlocked; }

    public void resetAnimanion()
    {
        animator.StopPlayback();
        animator.Play(0);
    }

}
