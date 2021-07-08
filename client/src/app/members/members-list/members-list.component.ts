import { Component, OnInit } from '@angular/core';
import { Observable } from 'rxjs';
import { take } from 'rxjs/operators';
import { Member } from 'src/app/models/member';
import { PaginatedResult, Pagination } from 'src/app/models/pagination';
import { User } from 'src/app/models/user';
import { UserParams } from 'src/app/models/usersParams';
import { AccountService } from 'src/app/_services/account.service';
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
  userParams!: UserParams;
  user!: User;
  // pageNumber = 1;
  // pageSize = 5;

  constructor(private memberService: MembersService, private accountService: AccountService) {
    this.accountService.currentUser$.pipe(take(1)).subscribe((user: User) => {
      this.user = user;
      this.userParams = new UserParams(user);
    })
  }

  ngOnInit(): void {
    this.loadMembers();
  }

  loadMembers() {
    this.memberService.getMembers(this.userParams).subscribe((response: PaginatedResult<Member[]>) => {
      // console.log('response', response);
      if(response.result != null){
        this.members =  response.result;
      }
      if(response.pagination != null){
        this.pagination = response.pagination;
      }
      // console.log({members: this.members});
    })
  }

  pageChanged(event: any ){
    this.userParams.pageNumber = event.page;
    this.loadMembers();
  }
}
