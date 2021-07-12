import { Component, OnInit, ViewChild } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { NgxGalleryOptions } from '@kolkov/ngx-gallery';
import { NgxGalleryImage } from '@kolkov/ngx-gallery';
import { NgxGalleryAnimation } from '@kolkov/ngx-gallery';
import { TabDirective, TabsetComponent } from 'ngx-bootstrap/tabs';
import { CreateMessage } from 'src/app/models/createMessage';
import { Member } from 'src/app/models/member';
import { Message } from 'src/app/models/message';
import { MessageService } from 'src/app/_services/message.service';
import { PresenceService } from 'src/app/_services/presence.service';

@Component({
  selector: 'app-member-details',
  templateUrl: './member-details.component.html',
  styleUrls: ['./member-details.component.scss'],
})
export class MemberDetailsComponent implements OnInit {
  @ViewChild('memberTabs', { static: true }) memberTabs!: TabsetComponent;

  activeTab!: TabDirective;
  member!: Member;
  galleryOptions!: NgxGalleryOptions[];
  galleryImages!: NgxGalleryImage[];

  messages: Message[] = [];

  constructor(
    public presenceService: PresenceService,
    private route: ActivatedRoute,
    private messageService: MessageService,
  ) {}

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
      this.loadMessages();
    }
  }

  sendMessage(event: CreateMessage) {
    this.messageService.sendMessage(event).subscribe((response: Message) => {
      this.messages.push(response);
    })
  }
}
