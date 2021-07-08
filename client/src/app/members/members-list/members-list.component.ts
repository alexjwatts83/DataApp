import { Component, OnInit } from '@angular/core';
import { Observable } from 'rxjs';
import { Member } from 'src/app/models/member';
import { PaginatedResult, Pagination } from 'src/app/models/pagination';
import { MembersService } from 'src/app/_services/members.service';

@Component({
  selector: 'app-members-list',
  templateUrl: './members-list.component.html',
  styleUrls: ['./members-list.component.scss']
})
export class MembersListComponent implements OnInit {
  // members$!: Observable<Member[]>;
  members!: Member[];
  pagination!: Pagination;
  pageNumber =1;
  pageSize = 5;
  constructor(private memberService: MembersService) {
    
   }

  ngOnInit(): void {
    // this.members$ = this.memberService.getMembers();
    this.loadMembers();
  }

  loadMembers() {
    this.memberService.getMembers(this.pageNumber, this.pageSize).subscribe((response: PaginatedResult<Member[]>) => {
      // console.log('response', response);
      if(response.result != null){
        this.members =  response.result;
      }
      if(response.pagination != null){
        this.pagination = response.pagination;
      }
      console.log({members: this.members});
    })
  }
}
