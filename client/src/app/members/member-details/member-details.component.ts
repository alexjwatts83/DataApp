import { Component, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { NgxGalleryOptions } from '@kolkov/ngx-gallery';
import { NgxGalleryImage } from '@kolkov/ngx-gallery';
import { NgxGalleryAnimation } from '@kolkov/ngx-gallery';
import { TabDirective, TabsetComponent } from 'ngx-bootstrap/tabs';
import { take } from 'rxjs/operators';
import { CreateMessage } from 'src/app/models/createMessage';
import { Member } from 'src/app/models/member';
import { Message } from 'src/app/models/message';
import { User } from 'src/app/models/user';
import { AccountService } from 'src/app/_services/account.service';
import { MessageService } from 'src/app/_services/message.service';
import { PresenceService } from 'src/app/_services/presence.service';

@Component({
  selector: 'app-member-details',
  templateUrl: './member-details.component.html',
  styleUrls: ['./member-details.component.scss'],
})
export class MemberDetailsComponent implements OnInit, OnDestroy {
  @ViewChild('memberTabs', { static: true }) memberTabs!: TabsetComponent;

  activeTab!: TabDirective;
  member!: Member;
  galleryOptions!: NgxGalleryOptions[];
  galleryImages!: NgxGalleryImage[];
  user!: User;

  messages: Message[] = [];
  isLoading = false;

  constructor(
    public presenceService: PresenceService,
    private route: ActivatedRoute,
    private messageService: MessageService,
    private accountService: AccountService,
    private router: Router
  ) {
    this.accountService.currentUser$.pipe(take(1)).subscribe((user: User) => {
      this.user = user;
    });
    this.router.routeReuseStrategy.shouldReuseRoute = () => false;
  }

  ngOnDestroy(): void {
    this.messageService.stopHubConnection();
  }

  ngOnInit(): void {
    this.route.data.subscribe(data => {
      this.member = data.member;
    })

    this.route.queryParams.subscribe(params => {
      params.tab ? this.selectTab(params.tab) : this.selectTab(0);
    });

    this.galleryOptions = [
      {
        width: '500px',
        height: '500px',
        imagePercent: 100,
        thumbnailsColumns: 4,
        imageAnimation: NgxGalleryAnimation.Slide,
        preview: false,
      },
    ];
    
    this.galleryImages = this.getGalleryImages();
  }

  getGalleryImages(): NgxGalleryImage[] {
    let galleryOptions: NgxGalleryImage[] = [];
    for (const image of this.member.photos) {
      galleryOptions.push({
        small: image?.url,
        medium: image?.url,
        big: image?.url,
      });
    }
    return galleryOptions;
  }

  loadMessages() {
    this.messageService.getMessageThread(this.member.username).subscribe((messages: Message[]) => {
      this.messages = messages;
    })
  }

  selectTab(tabId: number) {
    console.log( {memberTabs: this.memberTabs});
    if(this.memberTabs) {
      this.memberTabs.tabs[tabId].active = true;
    }
  }

  onTabActivated(data: TabDirective) {
    this.activeTab = data;
    if (this.activeTab.heading === 'Messages' && this.messages.length === 0) {
      // this.loadMessages();
      this.messageService.createHubConnection(this.user, this.member.username);
    } else {
      this.messageService.stopHubConnection();
    }
  }

  sendMessage(event: CreateMessage) {
    this.isLoading = true;
    this.messageService.sendMessage(event).then(() => {
      // this.messages.push(response);
    }).finally(() => {
      setTimeout(()=>{
        this.isLoading = false;
      }, 300);
    });
  }
}
