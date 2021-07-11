import { Component, EventEmitter, Input, OnInit, Output, ViewChild } from '@angular/core';
import { NgForm } from '@angular/forms';
import { CreateMessage } from 'src/app/models/createMessage';
import { Message } from 'src/app/models/message';

@Component({
  selector: 'app-member-messages',
  templateUrl: './member-messages.component.html',
  styleUrls: ['./member-messages.component.scss']
})
export class MemberMessagesComponent implements OnInit {
  @ViewChild('sendForm') sendForm!: NgForm;
  @Input() username: string = "";
  @Input() messages: Message[] = [];
  @Output() sendClick = new EventEmitter<CreateMessage>();
  
  content = '';

  constructor() { }

  ngOnInit(): void {}

  onSendClick() {
    if(this.content == null) {
      return;
    }
    let createMessage:CreateMessage = {
      recipientUsername: this.username,
      content: this.content
    };
    this.sendClick.emit(createMessage);
    this.sendForm.reset();
  }
}
