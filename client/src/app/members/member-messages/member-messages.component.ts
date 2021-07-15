import { ChangeDetectionStrategy, Component, EventEmitter, Input, OnInit, Output, ViewChild } from '@angular/core';
import { NgForm } from '@angular/forms';
import { Observable } from 'rxjs';
import { CreateMessage } from 'src/app/models/createMessage';
import { Message } from 'src/app/models/message';
import { MessageService } from 'src/app/_services/message.service';

@Component({
  changeDetection: ChangeDetectionStrategy.OnPush,
  selector: 'app-member-messages',
  templateUrl: './member-messages.component.html',
  styleUrls: ['./member-messages.component.scss']
})
export class MemberMessagesComponent implements OnInit {
  @ViewChild('sendForm') sendForm!: NgForm;
  @Input() username: string = "";
  @Input() isLoading: boolean = false;
  @Output() sendClick = new EventEmitter<CreateMessage>();

  messages$!: Observable<Message[]>;
  content = '';

  constructor(private messageService: MessageService) { }

  ngOnInit(): void {
    this.messages$ = this.messageService.messageThread$;
  }

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
