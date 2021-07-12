import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from 'src/environments/environment';
import { CreateMessage } from '../models/createMessage';
import { Message } from '../models/message';
import { getPaginatedResults, getPaginationHeaders } from './paginationHelper';

@Injectable({
  providedIn: 'root'
})
export class MessageService {
  private baseUrl = `${environment.apiUrl}/messages`;
  constructor(private http: HttpClient) { }

  private getUrl(path: string | number): string {
    return `${this.baseUrl}/${path}`;
  }

  getMessages(pageNumber: number, pageSize: number, container: string) {
    let params = getPaginationHeaders(pageNumber, pageSize);
    params = params.append('container', container);

    return getPaginatedResults<Message[]>(this.baseUrl, params, this.http);
  }

  getMessageThread(username: string) {
    return this.http.get<Message[]>(this.getUrl(`thread/${username}`));
  }

  sendMessage(messageParams: CreateMessage) {
    return this.http.post<Message>(this.baseUrl, messageParams);
  }

  deleteMessage(id: number) {
    return this.http.delete(this.getUrl(id));
  }
}