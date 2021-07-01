import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { Member } from 'src/app/models/member';
import { MembersService } from 'src/app/_services/members.service';

@Component({
  selector: 'app-member-details',
  templateUrl: './member-details.component.html',
  styleUrls: ['./member-details.component.scss']
})
export class MemberDetailsComponent implements OnInit {
  member!: Member;
  constructor(private memberService: MembersService, private route: ActivatedRoute) {
    
   }

  ngOnInit(): void {
    this.loadMember();
  }

  loadMember() {
    let username = this.route.snapshot.paramMap.get('username');
    if(username) {
      this.memberService.getMember(username).subscribe((member: Member) => {
        this.member = member;
      })
    } else {
      console.log('user not found');
    }
  }
}
