import { Injectable } from '@angular/core';
import { Router } from '@angular/router';
import * as signalR from '@microsoft/signalr';
import { HubConnection, HubConnectionBuilder } from '@microsoft/signalr';
import { ToastrService } from 'ngx-toastr';
import { BehaviorSubject, pipe } from 'rxjs';
import { take } from 'rxjs/operators';
import { environment } from 'src/environments/environment';
import { User } from '../models/user';

@Injectable({
  providedIn: 'root',
})
export class PresenceService {
  private hubUrl = `${environment.hubUrl}/presence`;
  private hubConnection!: HubConnection;
  private onlineUsersSource = new BehaviorSubject<string[]>([]);
  onlineUsers$ = this.onlineUsersSource.asObservable();

  constructor(private toastr: ToastrService, private router: Router) {}

  createHubConnection(user: User) {
    console.log('connecting to presence hub', user);

    const options = {
      accessTokenFactory: () => user.token,
      // skipNegotiation: true,
      transport: signalR.HttpTransportType.WebSockets,
    };
    this.hubConnection = new HubConnectionBuilder()
      .withUrl(this.hubUrl, options)
      .configureLogging(signalR.LogLevel.Information)
      .withAutomaticReconnect()
      .build();

    console.log({ hubConn: this.hubConnection });

    this.hubConnection
      .start()
      .then(() => console.log('Connected to hubs/presence'))
      .catch((error) => {
        this.toastr.error('Failed to connect to SignalR');
        console.log(error);
      });

    this.hubConnection.on('UserIsOnline', (username) => {
      this.toastr.info(username + ' has connected');
    });

    this.hubConnection.on('UserIsOffline', (username) => {
      this.toastr.warning(username + ' has disconnected');
    });

    this.hubConnection.on('GetOnlineUsers', (usernames: string[]) => {
      this.onlineUsersSource.next(usernames);
    });

    this.hubConnection.on('GetOnlineUsers', (usernames: string[]) => {
      this.onlineUsersSource.next(usernames);
    });

    this.hubConnection.on('NewMessageReceived', ({ username, knownAs }) => {
      console.log(knownAs, username);
      this.toastr
        .info(`${knownAs} has sent you message!`)
        .onTap.pipe(take(1))
        .subscribe(() =>
          this.router.navigateByUrl(`/members/${username}?tab=3`)
        );
    });
  }

  stopHubConnection() {
    this.hubConnection.stop().catch((error) => console.error(error));
  }
}
