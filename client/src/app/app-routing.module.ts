import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { HomeComponent } from './home/home.component';
import { ListsComponent } from './lists/lists.component';
import { MemberDetailsComponent } from './members/member-details/member-details.component';
import { MembersListComponent } from './members/members-list/members-list.component';
import { MessagesComponent } from './messages/messages.component';

const routes: Routes = [
  {
    path: '', component: HomeComponent
  },
  {
    path: 'members', component: MembersListComponent
  },
  {
    path: 'members/:id', component: MemberDetailsComponent
  },
  {
    path: 'lists', component: ListsComponent
  },
  {
    path: 'messages', component: MessagesComponent
  },
  {
    path: '**', component: HomeComponent, pathMatch: 'full'
  },
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
