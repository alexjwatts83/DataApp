import { Component, OnInit, ViewChild } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { NgxGalleryOptions } from '@kolkov/ngx-gallery';
import { NgxGalleryImage } from '@kolkov/ngx-gallery';
import { NgxGalleryAnimation } from '@kolkov/ngx-gallery';
import { TabDirective, TabsetComponent } from 'ngx-bootstrap/tabs';
import { Member } from 'src/app/models/member';
import { Message } from 'src/app/models/message';
import { MembersService } from 'src/app/_services/members.service';
import { MessageService } from 'src/app/_services/message.service';

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
    private memberService: MembersService,
    private route: ActivatedRoute,
    private messageService: MessageService,
  ) {}

  ngOnInit(): void {
    this.route.queryParams.subscribe(params => {
      console.log({queryParams: params});
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
    this.loadMember();
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

  loadMember() {
    let username = this.route.snapshot.paramMap.get('username');
    if (username) {
      this.memberService.getMember(username).subscribe((member: Member) => {
        this.member = member;
        this.galleryImages = this.getGalleryImages();
      });
    } else {
      console.log('user not found');
    }
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
}
