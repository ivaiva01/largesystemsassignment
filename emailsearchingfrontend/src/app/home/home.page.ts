import {Component} from '@angular/core';
import {ToastController} from "@ionic/angular";
import {HttpClient, HttpErrorResponse} from "@angular/common/http";
import {Email} from "../../models/Email";
import {environment} from "../../environments/environment";
import {firstValueFrom} from "rxjs";
import {ResponseDto} from "../../models/ResponseDto";

@Component({
  selector: 'app-home',
  templateUrl: 'home.page.html',
  styleUrls: ['home.page.scss'],
  standalone: false,
})
export class HomePage {
  searchterm?: string;
  allEmails: Email[] = [];

  constructor(private http: HttpClient, private toastController: ToastController) {
  }

  onKeydown($event: KeyboardEvent) {

    console.log(this.searchterm)
    if ($event.key === "Enter") {
      this.getEmailsWithTerm();
      this.allEmails = [
        { fileId: 1, emailBody: 'Don’t forget our meeting at 3 PM today.' },
        { fileId: 2, emailBody: 'Hey! Are we still up for the trip this weekend?' },
        { fileId: 3, emailBody: 'Your invoice for this month is attached.' },
        { fileId: 4, emailBody: 'Check out our latest updates and news.' }
      ];
    }
  }

  async getEmailsWithTerm() {
    try {
      console.log(this.searchterm);
      const observable = this.http.get<ResponseDto<Email[]>>(environment.baseUrl + '/email/search' + this.searchterm);
      const response = await firstValueFrom(observable);
      if (response != null) {
        const data = response.responseData!;
        //Add the emails to the allSortableElements array
        data.forEach((item: Email) => {
          this.allEmails.push(item);
          console.log(item.emailBody)
        });

        console.log(this.allEmails);
      }
      //Reset the search criteria, if field is empty
      this.searchterm = '';
    } catch (e) {
      this.createErrorToast(e);
    }
  }


  async createErrorToast(e: unknown) {
    //This error will be caught in the error interceptor
    if (e instanceof HttpErrorResponse) {
      if (e.status == 401) {
        const toast = await this.toastController.create({
          position: "middle",
          message: "An error occurred",
          duration: 4000,
          color: "danger"
        });
        toast.present();
      } else if (e.error.messageToClient = "Unknown Error") {
        const toast = await this.toastController.create({
          position: "bottom",
          message: "No contact to the backend",
          duration: 4000,
          color: "danger"
        });
        toast.present();
      } else {
        const toast = await this.toastController.create({
          position: "middle",
          message: e.error.messageToClient,
          duration: 4000,
          color: "danger"
        });
        toast.present();
      }
    }
  }
}
