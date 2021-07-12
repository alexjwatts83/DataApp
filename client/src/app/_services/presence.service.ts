import { Injectable } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { HubConnection, HubConnectionBuilder } from '@microsoft/signalr';
import { ToastrService } from 'ngx-toastr';
import { environment } from 'src/environments/environment';
import { User } from '../models/user';

@Injectable({
  providedIn: 'root',
})
export class PresenceService {
  private baseUrl = `${environment.hubUrl}/presence`;
  private hubConnection!: HubConnection;

  constructor(private toastr: ToastrService) {}

  createHubConnection(user: User) {
    console.log('connecting to hub', user);

    const options = {
      accessTokenFactory: () => user.token,
      // skipNegotiation: true,
      transport: signalR.HttpTransportType.WebSockets,
    };
    this.hubConnection = new HubConnectionBuilder()
      .withUrl(this.baseUrl, options)
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
  }

  stopHubConnection() {
    this.hubConnection.stop().catch((error) => console.error(error));
  }
}
