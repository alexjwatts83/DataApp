import { Component, OnInit } from '@angular/core';
import { BsModalRef } from 'ngx-bootstrap/modal';

@Component({
  selector: 'app-roles-modal',
  templateUrl: './roles-modal.component.html',
  styleUrls: ['./roles-modal.component.scss']
})
export class RolesModalComponent implements OnInit {
  title = 'blah';
  list: any[] = [];
  closeBtnName: string = 'close';
  constructor(public bsModalRef: BsModalRef) { }

  ngOnInit(): void {
  }

}
