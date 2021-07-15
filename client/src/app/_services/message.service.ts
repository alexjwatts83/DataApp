import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { HubConnection } from '@microsoft/signalr';
import { ToastrService } from 'ngx-toastr';
import { BehaviorSubject } from 'rxjs';
import { take } from 'rxjs/operators';
import { environment } from 'src/environments/environment';
import { CreateMessage } from '../models/createMessage';
import { Group } from '../models/group';
import { Message } from '../models/message';
import { User } from '../models/user';
import { BusyService } from './busy.service';
import { getPaginatedResults, getPaginationHeaders } from './paginationHelper';

@Injectable({
  providedIn: 'root',
})
export class MessageService {
  private baseUrl = `${environment.apiUrl}/messages`;
  private hubUrl = `${environment.hubUrl}/message`;
  private hubConnection!: HubConnection;

  private messageThreadSource = new BehaviorSubject<Message[]>([]);
  messageThread$ = this.messageThreadSource.asObservable();

  constructor(private http: HttpClient, private toastr: ToastrService, private busyService: BusyService) {}

  private getUrl(path: string | number): string {
    return `${this.baseUrl}/${path}`;
  }

  createHubConnection(user: User, otherUsername: string) {
    console.log('connecting to message hub', user);
    
    this.busyService.busy();

    const options = {
      accessTokenFactory: () => user.token,
      transport: signalR.HttpTransportType.WebSockets,
    };

    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(`${this.hubUrl}?user=${otherUsername}`, options)
      .configureLogging(signalR.LogLevel.Information)
      .withAutomaticReconnect()
      .build();

    this.hubConnection
      .start()
      .then(() => {
        console.log('Connected to hubs/message');
      })
      .catch((error) => {
        this.toastr.error('Failed to connect to hubs/message');
        console.log(error);
      }).finally(() => {
        this.busyService.idle();
      });

    this.hubConnection.on('RecievedMessageThread', (messages: Message[]) => {
      console.log('RecievedMessageThread', messages);
      this.messageThreadSource.next(messages);
    });

    this.hubConnection.on('NewMessage', (message: Message) => {
      console.log('NewMessage', message);
      this.messageThread$.pipe(take(1)).subscribe((messages: Message[]) => {
        this.messageThreadSource.next([...messages, message]);
      });
    });

    this.hubConnection.on('UpdatedGroup', (group: Group) => {
      if(group.connections.some(x => x.username === otherUsername)) {
        this.messageThread$.pipe(take(1)).subscribe((messages: Message[]) => {
          messages.forEach(message => {
            if(!message.dateRead) {
              message.dateRead = new Date(Date.now());
            }
          })
          this.messageThreadSource.next([...messages]);
        });
      }
    });
  }

  stopHubConnection() {
    if (!this.hubConnection) {
      return;
    }
    this.messageThreadSource.next([]);
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

  async sendMessage(messageParams: CreateMessage) {
    return this.hubConnection.invoke('SendMessage', messageParams).catch(error => console.log(error));
    // return this.http.post<Message>(this.baseUrl, messageParams);
  }

  deleteMessage(id: number) {
    return this.http.delete(this.getUrl(id));
  }
}
  