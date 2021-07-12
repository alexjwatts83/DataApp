import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { HubConnection } from '@microsoft/signalr';
import { ToastrService } from 'ngx-toastr';
import { BehaviorSubject } from 'rxjs';
import { environment } from 'src/environments/environment';
import { CreateMessage } from '../models/createMessage';
import { Message } from '../models/message';
import { User } from '../models/user';
import { getPaginatedResults, getPaginationHeaders } from './paginationHelper';

@Injectable({
  providedIn: 'root'
})
export class MessageService {
  private baseUrl = `${environment.apiUrl}/messages`;
  private hubUrl = `${environment.hubUrl}/message`;
  private hubConnection!: HubConnection;

  private messageThreadSource = new BehaviorSubject<Message[]>([]);
  messageThread$ = this.messageThreadSource.asObservable();

  constructor(private http: HttpClient, private toastr: ToastrService) { }

  private getUrl(path: string | number): string {
    return `${this.baseUrl}/${path}`;
  }

  createHubConnection(user: User, otherUsername: string) {
    console.log('connecting to message hub', user);

    const options = {
      accessTokenFactory: () => user.token,
      transport: signalR.HttpTransportType.WebSockets,
    };
    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(this.hubUrl, options)
      .configureLogging(signalR.LogLevel.Information)
      .withAutomaticReconnect()
      .build();

    console.log({ hubConn: this.hubConnection });

    this.hubConnection
      .start()
      .then(() => console.log('Connected to hubs/message'))
      .catch((error) => {
        this.toastr.error('Failed to connect to hubs/message');
        console.log(error);
      });

    this.hubConnection.on('RecievedMessageThread', (messages: Message[]) => {
      console.log('RecievedMessageThread', messages);
      this.messageThreadSource.next(messages);
    });

    // this.hubConnection.on('UserIsOffline', (username) => {
    //   this.toastr.warning(username + ' has disconnected');
    // });

    // this.hubConnection.on('GetOnlineUsers', (usernames: string[]) => {
    //   this.onlineUsersSource.next(usernames);
    // });
  }

  stopHubConnection() {
    this.hubConnection.stop().catch((error) => console.error(error));
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
