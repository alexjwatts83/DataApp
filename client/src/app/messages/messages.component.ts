import { Component, OnInit } from '@angular/core';
import { Message } from '../models/message';
import { PaginatedResult, Pagination } from '../models/pagination';
import { MessageService } from '../_services/message.service';

@Component({
  selector: 'app-messages',
  templateUrl: './messages.component.html',
  styleUrls: ['./messages.component.scss'],
})
export class MessagesComponent implements OnInit {
  messages!: Message[] | undefined;
  pagination: Pagination | undefined;
  container = 'unread';
  pageSize = 5;
  pageNumber = 1;
  isLoading = false;

  constructor(private messageService: MessageService) {}

  ngOnInit(): void {
    this.loadMessages();
  }

  loadMessages() {
    this.isLoading = true;
    this.messageService
      .getMessages(this.pageNumber, this.pageSize, this.container)
      .subscribe((response: PaginatedResult<Message[]>) => {
        this.messages = response.result;
        this.pagination = response.pagination;
        this.isLoading = false;
      });
  }

  pageChanged(event: any) {
    this.pageNumber = event.page;
    this.loadMessages();
  }

  deleteMessage(id: number) {
    this.messageService.deleteMessage(id).subscribe(() => {
      this.messages?.splice(this.messages.findIndex(m => m.id === id), 1);
    });
  }
}
