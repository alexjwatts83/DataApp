import { Component, OnInit } from '@angular/core';
import { Member } from '../models/member';
import { Pagination } from '../models/pagination';
import { LikesParams } from "../models/LikesParams";
import { MembersService } from '../_services/members.service';

@Component({
  selector: 'app-lists',
  templateUrl: './lists.component.html',
  styleUrls: ['./lists.component.scss']
})
export class ListsComponent implements OnInit {
  members!: Partial<Member[]> | undefined;
  likeParms = new LikesParams();
  pagination: Pagination | undefined;

  constructor(private memberService: MembersService) { 
  }

  ngOnInit(): void {
    this.loadLikes();
  }

  getTitle(): string {
    return this.likeParms.predicate === 'liked'
    ? 'Members I like'
    : 'Members who like me';
  }

  loadLikes() {
    this.memberService.getLikes(this.likeParms).subscribe(response => {
      this.members = response.result;
      this.pagination = response.pagination || undefined;
    });
  }

  pageChanged(event: any) {
    this.likeParms.pageNumber = event.page;
    this.loadLikes();
  }
}
