# Clouddns
I made this Cloudflare alternative to NoIP because the first thing is that I don't have to use another service. The second reason I did this is that I got a dynamic IP after switching ISPs and they make me pay a fee to have a static IP. *i hate paying fees*

## Prequisites
All you really need is a domain running off Cloudflare's nameservers and a computer that is capable of running the .NET 6.0 Runtime (pretty much anything ranging from a raspberry pi to a workstation).

## Setup
1. Get a domain. Can't afford one? Get a job. Don't have a job? Use Freenom.
2. Register Cloudflare's nameservers on the domain. I'm not going to go in depth on how this works because many domain services have different ways of doing this, but just make a Cloudflare account and they should tell you how to set it up. Regardless, if you're not using Cloudflare's nameservers on your own domain, you're cheating yourself, and Clouddns won't work without Cloudflare.
3. Get your domain's Zone ID. This can be found in the overview page if you scroll down:
    ![Image](https://media.discordapp.net/attachments/929570959688613918/936846065376821339/unknown.png)

    This will be used as argument `--zone`.
4. Click that one button called "Get your API token." Don't worry, I won't do anything to your domain; you can verify that by looking at my code. The program just needs this so it can change the address. When you create the token, use the "Edit zone DNS" template, and set its expiration dates if you want to. Make sure it can edit, and not only read. Keep the key somewhere safe, you'll be using this as argument `--token`.
5. Go to your site's DNS menu and make a subdomain with an A record. I'm just assuming that you have an IPv4 address like everyone else does. Put your current IP in there, any IP would work. You can choose if you want the reverse proxying, that basically just cloaks your IP so script kiddies can't pull your host's IP. This domain will be argument `--name`. Let me note if you use `@` as the name, you will need to use your domain name. If you provide a subdomain, the argument should be written as `sub.domain.tld`.
6. Get on your host computer, install .NET 6, and run this command:
    ```bash
    git clone https://github.com/LegitH3x0R/Clouddns
    cd Clouddns
    ```
7. Congratulations you cloned my project. Okay here's how to run it:
    ```bash
    dotnet run --token 1234 --zone abcd --name sub.domain.tld
    ```
8. Profit?

## CLI Arguments
```
-r, --rate     (Default: 10) Check rate. Expects time in seconds.
-t, --token    Required. Cloudflare API token.
-z, --zone     Required. Domain Zone ID.
-n, --name     Required. DNS record name.
--help         Display this help screen.
--version      Display version information.
```

# License
This is under the MIT license so you can do anything with my spaghetti code. Improve it if you're up for the task, I didn't bother to revise it. I don't care if you use it to profit. It was more of a benefit for me, and anyone else who might find this useful.