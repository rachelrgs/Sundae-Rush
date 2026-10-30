Rachel Rogers
CS 583: 3D Game Programming

Sundae Rush is a 2D time-management game. You play as a new employee at a struggling ice cream parlor and help turn the business around by making the right sundaes for customers before they lose their patience.


CHANGES FROM THE ORIGINAL ONE-PAGER

1. Fewer rounds
The one-pager planned 15 rounds, looping after every 5. I noted at the time that this might be too ambitious, so the final game has 3 rounds.

2. Lives instead of a review meter that can drop
Originally, the player would lose if the review meter dropped to zero. In the final game, a correct order adds a good review, and a wrong order or a customer who runs out of patience counts as a bad review and costs a life. The player has 3 lives, and lives carry over between rounds instead of resetting.

3. New menu items instead of upgrades
The one-pager had the boss offer an upgrade after rounds 5 and 10, such as more customer patience or a faster tool. The final game drops upgrades. Instead, the boss congratulates the player after Round 1 and Round 2 and adds one new flavor, one new sauce, and one new topping each time:
- Round 1: Chocolate, Vanilla, Strawberry / Hot Fudge, Caramel / Whipped Cream, Cherries
- Round 2 adds: Mint Chip / Strawberry sauce / Nuts
- Round 3 adds: Coffee / Marshmallow Fluff / Sprinkles

4. Simpler orders
The one-pager included occasional large, complicated sundae orders worth more points. The final game removes these. Every order is randomly generated and is always 1 scoop, 1 sauce, and 1 topping. The player clicks a flavor, then a sauce, then a topping, then clicks the finished sundae and the customer who ordered it. Clicking the cones throws the sundae away so the player can start over.

5. Difficulty
The second draft planned 10, 12, and 15 correct orders for Rounds 1 to 3. At the last minute I changed this to 10 good reviews for every round. Rounds still get harder because customers arrive more often (every 8, 6.5, then 5 seconds), have less patience (30, 25, then 20 seconds), and there are more menu items to choose from.

6. Visible patience timer
Each customer has a timer next to their head that counts down the seconds. It turns red in the last 5 seconds, and the customer's face changes to an angry expression as time runs low.

7. Story and round screens
The introduction, round info, and boss messages are shown as cream-colored 8-bit text on a pastel green screen. Before each round, the parlor background is shown for a few seconds, followed by a screen listing the new items. If the player loses all 3 lives, the parlor goes out of business and the player starts over from Round 1.

8. Fewer sounds
The one-pager planned separate sounds for scooping, placing toppings, completing an order, an incorrect order, a customer arriving, and finishing a round. The final game uses three: a click sound for interacting with items, a correct-order sound, and an incorrect-order sound.

9. Smaller art list
Upgrade icons are no longer needed. The game is viewed from behind the counter, so an employee sprite is not used either. Hot Fudge uses the chocolate sauce art.


CREDITS
Font: Press Start 2P by CodeMan38
Sound effects: Casual Game Sounds U6 pack.
I made the assets myself.


AI DISCLOSURE

I used Claude to help build this project. I created the logic, and used Claude to assist with the game code and set up the click areas to match my artwork. The game concept, design, and art and sound assets were provided by me. I reviewed the and understand all of the code.